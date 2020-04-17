using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SnaffCore.Concurrency
{
    public class BlockingStaticTaskScheduler
    {
        // singleton cruft
        //private static BlockingStaticTaskScheduler _instance;
        private static readonly object syncLock = new object();

        // constructor
        public BlockingStaticTaskScheduler(int threads, int maxBacklog)
        {
            _scheduler = new LimitedConcurrencyLevelTaskScheduler(threads);
            _taskFactory = new TaskFactory(_scheduler);
            _cancellationSource = new CancellationTokenSource();
            _maxBacklog = maxBacklog;
        }

        // task factory things!!!
        private LimitedConcurrencyLevelTaskScheduler _scheduler { get; }
        private TaskFactory _taskFactory { get; }
        private CancellationTokenSource _cancellationSource { get; }
        private int _maxBacklog { get; set; }

        public int CurrentTasksRunning { get; set; } = 0;
        public int CurrentTasksQueued { get; set; } = 0;
        public int TotalTasksQueued { get; set; } = 0;

        public void New(Action action)
        {
            // set up to not add the task as default
            bool proceed = false;

            while (proceed == false) // loop the calling thread until we are allowd to do the thing
            {
                lock (syncLock) // take out the lock
                {
                    // update numbers
                    CurrentTasksQueued = _scheduler._tasks.Count;
                    CurrentTasksRunning = _scheduler._delegatesQueuedOrRunning;
                    TotalTasksQueued = _scheduler._totalTasksQueued;
                    
                    // check to see how many tasks we have waiting and keep looping if it's too many
                    if (CurrentTasksQueued >= _maxBacklog)
                        continue;

                    // okay, let's add the thing
                    proceed = true;
                    _taskFactory.StartNew(action, _cancellationSource.Token);
                }
            }
        }

        /*
        public static BlockingStaticTaskScheduler Use()
        {
            if (_instance == null)
            {
                lock (syncLock)
                {
                    if (_instance == null)
                    {
                        throw new InvalidOperationException("This singleton must be instantiated with .Use(int threads, int maxBacklog) overload before use");
                    }
                }
            }
            return _instance;
        }

        public static BlockingStaticTaskScheduler Use(int threads, int maxBacklog)
        {
            if (_instance == null)
            {
                lock (syncLock)
                {
                    if (_instance == null)
                    {
                        _instance = new BlockingStaticTaskScheduler(threads, maxBacklog);
                    }
                    else
                    {
                        throw new InvalidOperationException("This singleton should only be instantiated once");
                    }
                }
            }

            return _instance;
        }
        */
    }

    internal class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
    {
        // Indicates whether the current thread is processing work items.
        [ThreadStatic] private static bool _currentThreadIsProcessingItems;

        // The maximum concurrency level allowed by this scheduler. 

        // The list of tasks to be executed 
        public readonly LinkedList<Task> _tasks = new LinkedList<Task>(); // protected by lock(_tasks)

        // Indicates whether the scheduler is currently processing work items. 
        public int _delegatesQueuedOrRunning;
        public int _totalTasksQueued;

        // Creates a new instance with the specified degree of parallelism. 
        public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1) throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism));
            MaximumConcurrencyLevel = maxDegreeOfParallelism;
        }

        // Gets the maximum concurrency level supported by this scheduler. 
        public sealed override int MaximumConcurrencyLevel { get; }

        // Queues a task to the scheduler. 
        protected sealed override void QueueTask(Task task)
        {
            // Add the task to the list of tasks to be processed.  If there aren't enough 
            // delegates currently queued or running to process tasks, schedule another. 
            lock (_tasks)
            {
                _tasks.AddLast(task);
                _totalTasksQueued++;
                if (_delegatesQueuedOrRunning < MaximumConcurrencyLevel)
                {
                    ++_delegatesQueuedOrRunning;
                    NotifyThreadPoolOfPendingWork();
                }
            }
        }

        // Inform the ThreadPool that there's work to be executed for this scheduler. 
        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
                // Note that the current thread is now processing work items.
                // This is necessary to enable inlining of tasks into this thread.
                _currentThreadIsProcessingItems = true;
                try
                {
                    // Process all available items in the queue.
                    while (true)
                    {
                        Task item;
                        lock (_tasks)
                        {
                            // When there are no more items to be processed,
                            // note that we're done processing, and get out.
                            if (_tasks.Count == 0)
                            {
                                --_delegatesQueuedOrRunning;
                                break;
                            }

                            // Get the next item from the queue
                            item = _tasks.First.Value;
                            _tasks.RemoveFirst();
                        }

                        // Execute the task we pulled out of the queue
                        TryExecuteTask(item);
                    }
                }
                // We're done processing items on the current thread
                finally
                {
                    _currentThreadIsProcessingItems = false;
                }
            }, null);
        }

        // Attempts to execute the specified task on the current thread. 
        protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            // If this thread isn't already processing a task, we don't support inlining
            if (!_currentThreadIsProcessingItems) return false;

            // If the task was previously queued, remove it from the queue
            if (taskWasPreviouslyQueued)
                // Try to run the task. 
                if (TryDequeue(task))
                    return TryExecuteTask(task);
                else
                    return false;
            return TryExecuteTask(task);
        }

        // Attempt to remove a previously scheduled task from the scheduler. 
        protected sealed override bool TryDequeue(Task task)
        {
            lock (_tasks)
            {
                return _tasks.Remove(task);
            }
        }

        // Gets an enumerable of the tasks currently scheduled on this scheduler. 
        protected sealed override IEnumerable<Task> GetScheduledTasks()
        {
            var lockTaken = false;
            try
            {
                Monitor.TryEnter(_tasks, ref lockTaken);
                if (lockTaken) return _tasks;
                else throw new NotSupportedException();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_tasks);
            }
        }
    }
}