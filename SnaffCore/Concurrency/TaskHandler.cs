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
        private static LimitedConcurrencyLevelTaskScheduler _shareFinderLclts;
        private static LimitedConcurrencyLevelTaskScheduler _treeWalkerLclts;
        private static LimitedConcurrencyLevelTaskScheduler _fileScannerLclts;
        private static TaskFactory _shareFinderTaskFactory;
        private static TaskFactory _treeWalkerTaskFactory;
        private static TaskFactory _fileScannerTaskFactory;
        private static CancellationTokenSource _shareFinderCts;
        private static CancellationTokenSource _treeWalkerCts;
        private static CancellationTokenSource _fileScannerCts;

        public static void CreateLCLTSes(int maxDegreeOfParallelism)
        {
            _shareFinderLclts = new LimitedConcurrencyLevelTaskScheduler(maxDegreeOfParallelism);
            _treeWalkerLclts = new LimitedConcurrencyLevelTaskScheduler(maxDegreeOfParallelism);
            _fileScannerLclts = new LimitedConcurrencyLevelTaskScheduler(maxDegreeOfParallelism);
            _shareFinderTaskFactory = new TaskFactory(_shareFinderLclts);
            _treeWalkerTaskFactory = new TaskFactory(_treeWalkerLclts);
            _fileScannerTaskFactory = new TaskFactory(_fileScannerLclts);
            _shareFinderCts = new CancellationTokenSource();
            _treeWalkerCts = new CancellationTokenSource();
            _fileScannerCts = new CancellationTokenSource();
        }

        public static LimitedConcurrencyLevelTaskScheduler GetShareFinderLCLTS()
        {
            return _shareFinderLclts;
        }

        public static LimitedConcurrencyLevelTaskScheduler GetTreeWalkerLCLTS()
        {
            return _treeWalkerLclts;
        }

        public static LimitedConcurrencyLevelTaskScheduler GetFileScannerLCTLS()
        {
            return _fileScannerLclts;
        }

        public static TaskFactory GetShareFinderTaskFactory()
        {
            return _shareFinderTaskFactory;
        }

        public static TaskFactory GetTreeWalkerTaskFactory()
        {
            return _treeWalkerTaskFactory;
        }

        public static TaskFactory GetFileScannerTaskFactory()
        {
            return _fileScannerTaskFactory;
        }

        public static CancellationTokenSource GetShareFinderCts()
        {
            return _shareFinderCts;
        }

        public static CancellationTokenSource GetTreeWalkerCts()
        {
            return _treeWalkerCts;
        }

        public static CancellationTokenSource GetFileScannerCts()
        {
            return _fileScannerCts;
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