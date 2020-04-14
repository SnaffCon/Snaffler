using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SnaffCore.Concurrency
{
    // Provides a task scheduler that ensures a maximum concurrency level while 
    // running on top of the thread pool.

    public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
    {
        private static LimitedConcurrencyLevelTaskScheduler _shareLclts;
        private static TaskFactory _shareTaskFactory;
        private static CancellationTokenSource _shareCts;
        private static LimitedConcurrencyLevelTaskScheduler _treeLclts;
        private static TaskFactory _treeTaskFactory;
        private static CancellationTokenSource _treeCts;
        private static LimitedConcurrencyLevelTaskScheduler _fileLclts;
        private static TaskFactory _fileTaskFactory;
        private static CancellationTokenSource _fileCts;
        private static List<Task> _snafflerTaskList { get; set; } = new List<Task>();

        public static void CreateLCLTSes(int maxDegreeOfParallelism)
        {
            int shareThreads = maxDegreeOfParallelism / 10;
            int treeThreads = maxDegreeOfParallelism / 5;
            int fileThreads = maxDegreeOfParallelism;

            _shareLclts = new LimitedConcurrencyLevelTaskScheduler(shareThreads);
            _shareTaskFactory = new TaskFactory(_shareLclts);
            _shareCts = new CancellationTokenSource();

            _treeLclts = new LimitedConcurrencyLevelTaskScheduler(treeThreads);
            _treeTaskFactory = new TaskFactory(_treeLclts);
            _treeCts = new CancellationTokenSource();

            _fileLclts = new LimitedConcurrencyLevelTaskScheduler(fileThreads);
            _fileTaskFactory = new TaskFactory(_fileLclts);
            _fileCts = new CancellationTokenSource();
        }

        public static TaskFactory GetShareTaskFactory()
        {
            return _shareTaskFactory;
        }
        public static CancellationTokenSource GetShareCts()
        {
            return _shareCts;
        }
        public static TaskFactory GetFileTaskFactory()
        {
            return _fileTaskFactory;
        }
        public static CancellationTokenSource GetFileCts()
        {
            return _fileCts;
        }
        public static TaskFactory GetTreeTaskFactory()
        {
            return _treeTaskFactory;
        }
        public static CancellationTokenSource GetTreeCts()
        {
            return _treeCts;
        }
        public static List<Task> GetSnafflerTaskList()
        {
            return _snafflerTaskList;
        }

        // Indicates whether the current thread is processing work items.
        [ThreadStatic] private static bool _currentThreadIsProcessingItems;

        // The list of tasks to be executed 
        private readonly LinkedList<Task> _tasks = new LinkedList<Task>(); // protected by lock(_tasks)

        // The maximum concurrency level allowed by this scheduler. 
        private readonly int _maxDegreeOfParallelism;

        // Indicates whether the scheduler is currently processing work items. 
        private int _delegatesQueuedOrRunning;

        // Creates a new instance with the specified degree of parallelism. 
        private LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1) throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism));
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        // Queues a task to the scheduler. 
        protected sealed override void QueueTask(Task task)
        {
            // Add the task to the list of tasks to be processed.  If there aren't enough 
            // delegates currently queued or running to process tasks, schedule another. 
            lock (_tasks)
            {
                _snafflerTaskList.Add(task);
                _tasks.AddLast(task);
                if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
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
            lock (_tasks) return _tasks.Remove(task);
        }

        // Gets the maximum concurrency level supported by this scheduler. 
        public sealed override int MaximumConcurrencyLevel => _maxDegreeOfParallelism;

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