using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SnaffCore.Concurrency
{
    public class BlockingStaticTaskScheduler
    {
        // singleton cruft
        private static readonly object syncLock = new object();

        //public TaskCounters TaskCounters { get; set; }

        // task factory things!!!
        public LimitedConcurrencyLevelTaskScheduler Scheduler { get; }
        private TaskFactory _taskFactory { get; }
        private CancellationTokenSource _cancellationSource { get; }
        private int _maxBacklog { get; set; }

        // constructor
        public BlockingStaticTaskScheduler(int threads, int maxBacklog)
        {
            Scheduler = new LimitedConcurrencyLevelTaskScheduler(threads);
            _taskFactory = new TaskFactory(Scheduler);
            _cancellationSource = new CancellationTokenSource();
            _maxBacklog = maxBacklog;
        }

        public bool Done()
        {
            // single get, it's locked inside the method
            Scheduler.RecalculateCounters();
            TaskCounters taskCounters = Scheduler.GetTaskCounters();

            Console.WriteLine($"Checking if done - queued: {taskCounters.CurrentTasksQueued}, done: {taskCounters.CurrentTasksRunning}");

            if ((taskCounters.CurrentTasksQueued + taskCounters.CurrentTasksRunning == 0))
            {
                return true;
            }

            return false;
        }
        public void New(Action action)
        {
            // set up to not add the task as default
            bool proceed = false;

            while (proceed == false) // loop the calling thread until we are allowed to do the thing
            {
                lock (syncLock) // take out the lock
                {
                    // check to see how many tasks we have waiting and keep looping if it's too many
                    // single get, it's locked inside the method.
                    // _maxBacklog = 0 is 'infinite'
                    if (_maxBacklog != 0)
                    {
                        if (Scheduler.GetTaskCounters().CurrentTasksQueued >= _maxBacklog)
                            continue;
                    }

                    // okay, let's add the thing
                    proceed = true;

                    _taskFactory.StartNew(action, _cancellationSource.Token);
                }
            }
        }
    }

    public enum TaskType
    {
        None = 0,
        Share = 1,
        Tree = 2,
        File = 3
    }

    public struct TaskFileEntry
    {
        public TaskFileEntryStatus status;
        public string guid;
        public TaskType type;
        public string input;

        public TaskFileEntry(string entryLine)
        {
            string[] lineParts = entryLine.Split('|');

            status = (TaskFileEntryStatus)Enum.Parse(typeof(TaskFileEntryStatus), lineParts[0]);
            guid = lineParts[1];

            if (status == TaskFileEntryStatus.Pending)
            {
                type = (TaskType)Enum.Parse(typeof(TaskType), lineParts[2]);
                input = lineParts[3];
            }
            else
            {
                type = TaskType.None;
                input = null;
            }
        }
    }

    public enum TaskFileEntryStatus
    {
        Pending = 0,
        Completed = 1,
    }

    public class ResumingTaskScheduler : BlockingStaticTaskScheduler
    {
        private static readonly object WriteLock = new object();
        private static StreamWriter fileWriter;

        private static string[] AlreadyHandledTasks;

        internal BlockingMq Mq { get; }

        public ResumingTaskScheduler(int threads, int maxBacklog) : base(threads, maxBacklog)
        {
            this.Mq = BlockingMq.GetMq();
        }

        public static void SetAlreadyHandledTasks(TaskFileEntry[] taskFileEntries)
        {
            AlreadyHandledTasks = taskFileEntries.Select(entry => entry.input).ToArray();
        }

        public static bool IsTaskAlreadyHandled(string input)
        {
            if (AlreadyHandledTasks == null) return false;
            return AlreadyHandledTasks.Contains(input);
        }

        public void New(TaskType taskType, Action<string> action, string input)
        {
            if (IsTaskAlreadyHandled(input)) return;

            string taskGuid = SaveTask(taskType, input);

            New(() =>
            {
                try
                {
                    action(input);
                }
                catch (Exception e)
                {
                    Mq.Error("Exception in " + taskType.ToString() + " task for host " + input);
                    Mq.Error(e.ToString());
                }

                CompleteTask(taskGuid);
            });
        }

        public static void SetTaskFile(string path)
        {
            fileWriter = new StreamWriter(path);
        }

        public static void CloseTaskFile()
        {
            if (fileWriter != null) {
                lock (WriteLock)
                {
                    fileWriter.Close();
                    fileWriter = null;
                }
            }
        }

        internal string SaveTask(TaskType taskType, string input)
        {
            // task file is not set, we are not saving tasks
            if (fileWriter == null) return null;

            string taskGuid = Guid.NewGuid().ToString();

            StringBuilder taskEntryBuilder = new StringBuilder();

            taskEntryBuilder.Append("Pending|");
            taskEntryBuilder.Append(taskGuid);
            taskEntryBuilder.Append("|");
            taskEntryBuilder.Append(taskType.ToString());
            taskEntryBuilder.Append("|");
            taskEntryBuilder.Append(input);

            lock (WriteLock)
            {
                fileWriter.WriteLine(taskEntryBuilder.ToString());
                fileWriter.Flush();
            }

            return taskGuid;
        }

        internal void CompleteTask(string taskGuid)
        {
            if (fileWriter == null) return;

            lock (WriteLock)
            {
                fileWriter.WriteLine("Completed|" + taskGuid);
                fileWriter.Flush();
            }
        }
    }

    public class ShareTaskScheduler : ResumingTaskScheduler
    {
        public ShareTaskScheduler(int threads, int maxBacklog) : base(threads, maxBacklog) { }

        public void New(Action<string> action, string file)
        {
            New(TaskType.Share, action, file);
        }
    }

    public class TreeTaskScheduler : ResumingTaskScheduler
    {
        public TreeTaskScheduler(int threads, int maxBacklog) : base(threads, maxBacklog) { }

        public void New(Action<string> action, string file)
        {
            New(TaskType.Tree, action, file);
        }
    }

    public class FileTaskScheduler : ResumingTaskScheduler
    {
        public FileTaskScheduler(int threads, int maxBacklog) : base(threads, maxBacklog) { }

        public void New(Action<string> action, string file)
        {
            New(TaskType.File, action, file);
        }
    }

    public class TaskCounters
    {
        public BigInteger TotalTasksQueued { get; set; }
        public BigInteger CurrentTasksQueued { get; set; }
        public BigInteger CurrentTasksRunning { get; set; }
        public BigInteger CurrentTasksRemaining { get; set; }
        public BigInteger CompletedTasks { get; set; }
        public int MaxParallelism { get; set; }
    }

    public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
    {
        public TaskCounters _taskCounters { get; private set; }

        public TaskCounters GetTaskCounters()
        {
            lock (_taskCounters)
            {
                return _taskCounters;
            }
        }
        public void RecalculateCounters()
        {
            lock (_taskCounters)
            {
                this._taskCounters.CurrentTasksQueued = _tasks.Count;
                this._taskCounters.CurrentTasksRunning = _delegatesQueuedOrRunning;
                this._taskCounters.CurrentTasksRemaining = this._taskCounters.CurrentTasksQueued + this._taskCounters.CurrentTasksRunning;
                this._taskCounters.CompletedTasks = this._taskCounters.TotalTasksQueued - this._taskCounters.CurrentTasksRemaining;
                this._taskCounters.MaxParallelism = this._maxDegreeOfParallelism;
            }
        }

        // Indicates whether the current thread is processing work items.
        [ThreadStatic] private static bool _currentThreadIsProcessingItems;

        // The list of tasks to be executed 
        public readonly LinkedList<Task> _tasks = new LinkedList<Task>();

        // The maximum concurrency level allowed by this scheduler. 
        public int _maxDegreeOfParallelism;

        // Indicates whether the scheduler is currently processing work items. 
        private int _delegatesQueuedOrRunning;

        // Creates a new instance with the specified degree of parallelism. 
        public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1) throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism));
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
            this._taskCounters = new TaskCounters();
        }

        // Queues a task to the scheduler. 
        protected sealed override void QueueTask(Task task)
        {
            // Add the task to the list of tasks to be processed.  If there aren't enough 
            // delegates currently queued or running to process tasks, schedule another. 
            lock (_tasks)
            {
                _tasks.AddLast(task);
                ++_taskCounters.TotalTasksQueued;
                if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                {
                    ++_delegatesQueuedOrRunning;
                    NotifyThreadPoolOfPendingWork();
                }
                RecalculateCounters();
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
                            RecalculateCounters();
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
                {
                    RecalculateCounters();
                    return TryExecuteTask(task);
                }
                else
                    return false;
            return TryExecuteTask(task);
        }

        // Gets the maximum concurrency level supported by this scheduler. 
        public sealed override int MaximumConcurrencyLevel => _maxDegreeOfParallelism;

        // Gets an enumerable of the tasks currently scheduled on this scheduler. 
        protected sealed override IEnumerable<Task> GetScheduledTasks()
        {
            bool lockTaken = false;
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
