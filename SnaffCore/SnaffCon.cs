using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using SnaffCore.ComputerFind;
using SnaffCore.Concurrency;
using SnaffCore.ShareFind;
using SnaffCore.TreeWalk;
using SnaffCore.Config;
using static SnaffCore.Config.Options;
using Timer = System.Timers.Timer;

namespace SnaffCore
{
    public class SnaffCon
    {
        private bool AllTasksComplete { get; set; } = false;
        private BlockingMq Mq { get; set; }

        private object StatusObjectLocker = new object();

        private static LimitedConcurrencyLevelTaskScheduler _shareLclts;
        private static TaskFactory _shareTaskFactory;
        private static CancellationTokenSource _shareCts;
        private static LimitedConcurrencyLevelTaskScheduler _treeLclts;
        private static TaskFactory _treeTaskFactory;
        private static CancellationTokenSource _treeCts;
        private static LimitedConcurrencyLevelTaskScheduler _fileLclts;
        private static TaskFactory _fileTaskFactory;
        private static CancellationTokenSource _fileCts;
        private int CompletedFileTaskCounter;
        private int RemainingFileTaskCounter;
        private int CompletedShareTaskCounter;
        private int RemainingShareTaskCounter;
        private int CompletedTreeTaskCounter;
        private int RemainingTreeTaskCounter;

        public SnaffCon(Options options)
        {
            MyOptions = options;
            Mq = BlockingMq.GetMq();

            int threads = MyOptions.MaxThreads;
            int shareThreads = threads / 4;
            int treeThreads = threads / 10;
            int fileThreads = threads;

            _shareLclts = new LimitedConcurrencyLevelTaskScheduler(shareThreads);
            _shareTaskFactory = new TaskFactory(_shareLclts);
            _shareCts = new CancellationTokenSource();

            _treeLclts = new LimitedConcurrencyLevelTaskScheduler(treeThreads);
            _treeTaskFactory = new TaskFactory(_treeLclts);
            _treeCts = new CancellationTokenSource();

            _fileLclts = new LimitedConcurrencyLevelTaskScheduler(fileThreads);
            _fileTaskFactory = new TaskFactory(_fileLclts);
            _fileCts = new CancellationTokenSource();

            CompletedFileTaskCounter = 0;
            RemainingFileTaskCounter = 0;
            CompletedShareTaskCounter = 0;
            RemainingShareTaskCounter = 0;
            CompletedTreeTaskCounter = 0;
            RemainingTreeTaskCounter = 0;
        }

        public void Execute()
        {
            // This is the main execution thread.
            Timer statusUpdateTimer =
                new Timer(TimeSpan.FromMinutes(0.5)
                    .TotalMilliseconds) {AutoReset = true}; // Set the time (1 min in this case)
            statusUpdateTimer.Elapsed += StatusUpdate;
            statusUpdateTimer.Start();

            // if we haven't been told what dir or computer to target, we're going to need to do share discovery. that means finding computers from the domain.
            if (MyOptions.PathTargets == null && MyOptions.ComputerTargets == null)
            {
                ComputerDiscovery();
            }
            // if we've been told what computers to hit...
            else if (MyOptions.ComputerTargets != null)
            {
                ShareDiscovery(MyOptions.ComputerTargets);
            }
            // otherwise we should have a set of path targets...
            else if (MyOptions.PathTargets != null)
            {
                FileDiscovery(MyOptions.PathTargets);
            }
            // but if that hasn't been done, something has gone wrong.
            else
            {
                Mq.Error("OctoParrot says: AWK! I SHOULDN'T BE!");
            }

            // TODO (NOT THIS)
            while (!AllTasksComplete)
            {
                // lol whaaaayy
            }

            Mq.Info("Finished!");
            Console.ResetColor();
            Environment.Exit(0);
        }

        private void ComputerDiscovery()
        {
            Mq.Info("Getting computers from AD.");
            // We do this single threaded cos it's fast and not easily divisible.
            var activeDirectory = new ActiveDirectory();
            List<string> targetComputers = activeDirectory.DomainComputers;
            if (targetComputers == null)
            {
                Mq.Error(
                    "Something fucked out finding the computers in the domain. You must be holding it wrong.");
                while (true)
                {
                    Mq.Terminate();
                }
            }
            var numTargetComputers = targetComputers.Count.ToString();
            Mq.Info("Got " + numTargetComputers + " computers from AD.");
            if (targetComputers.Count == 0)
            {
                Mq.Error("Didn't find any domain computers. Seems weird. Try pouring water on it.");
                while (true)
                {
                    Mq.Terminate();
                }
            }
            // immediately call ShareDisco which should find the rest.
            ShareDiscovery(targetComputers.ToArray());
        }

        private void ShareDiscovery(string[] computerTargets)
        {
            TaskFactory SharefinderTaskFactory = GetShareTaskFactory();
            CancellationTokenSource SharefinderCts = GetShareCts();

            Mq.Info("Starting to find readable shares.");
            foreach (var computer in computerTargets)
            {
                // ShareFinder Task Creation - this kicks off the rest of the flow
                Mq.Info("Creating a sharefinder task for " + computer);
                var t = SharefinderTaskFactory.StartNew(() =>
                {
                    try
                    {
                        ShareFinder shareFinder = new ShareFinder();
                        shareFinder.GetComputerShares(computer);
                    }
                    catch (Exception e)
                    {
                        Mq.Trace(e.ToString());
                    }
                }, SharefinderCts.Token);
            }
            Mq.Info("Created all sharefinder tasks.");
        }

        private void FileDiscovery(string[] pathTargets)
        {
            TaskFactory SharescannerTaskFactory = GetShareTaskFactory();
            CancellationTokenSource SharescannerCts = GetShareCts();

            foreach (string pathTarget in pathTargets)
            {
                // ShareScanner Task Creation - this kicks off the rest of the flow
                Mq.Info("Creating a TreeWalker task for " + pathTarget);
                var t = SharescannerTaskFactory.StartNew(() =>
                {
                    try
                    {
                        TreeWalker treeWalker = new TreeWalker(pathTarget);
                    }
                    catch (Exception e)
                    {
                        Mq.Trace(e.ToString());
                    }
                }, SharescannerCts.Token);
            }

            Mq.Info("Created all TreeWalker tasks.");
        }

        private void TaskCleanup()
        {
                // get the completed stuff from the last minute
                //Task[] completedTasks = Array.FindAll(SnafflerTasks.ToArray(),
                 //   element => element.Status == TaskStatus.RanToCompletion);
        }

        // This method is called every minute
        private void StatusUpdate(object sender, ElapsedEventArgs e)
        {
            lock (StatusObjectLocker)
            {
                // get memory usage for status update
                string memorynumber;
                using (Process proc = Process.GetCurrentProcess())
                {
                    long memorySize64 = proc.PrivateMemorySize64;
                    memorynumber = BytesToString(memorySize64);
                }

                RemainingFileTaskCounter = _fileLclts._delegatesQueuedOrRunning + _fileLclts.CurrentTasksQueued();
                CompletedFileTaskCounter = _fileLclts._totalTasksQueued - RemainingFileTaskCounter;

                RemainingShareTaskCounter = _shareLclts._delegatesQueuedOrRunning + _shareLclts.CurrentTasksQueued();
                CompletedShareTaskCounter = _shareLclts._totalTasksQueued - RemainingShareTaskCounter;

                RemainingTreeTaskCounter = _treeLclts._delegatesQueuedOrRunning + _treeLclts.CurrentTasksQueued();
                CompletedTreeTaskCounter = _treeLclts._totalTasksQueued - RemainingTreeTaskCounter;

                var updateText = new StringBuilder("Status Update: \n");

                updateText.Append("ShareFinder Tasks Completed: " + CompletedShareTaskCounter + "\n");
                updateText.Append("ShareFinder Tasks Remaining: " + RemainingShareTaskCounter + "\n");
                updateText.Append("TreeWalker Tasks Completed: " + CompletedTreeTaskCounter + "\n");
                updateText.Append("TreeWalker Tasks Remaining: " + RemainingTreeTaskCounter + "\n");
                updateText.Append("FileScanner Tasks Completed: " + CompletedFileTaskCounter + "\n");
                updateText.Append("FileScanner Tasks Remaining: " + RemainingFileTaskCounter + "\n");
                updateText.Append(memorynumber + " RAM in use.");

                Mq.Info(updateText.ToString());

                if ((RemainingFileTaskCounter + RemainingShareTaskCounter + RemainingTreeTaskCounter) == 0)
                {
                    AllTasksComplete = true;
                }
            }
        }

        private static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "kB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num) + suf[place];
        }

        public static LimitedConcurrencyLevelTaskScheduler GetTreeLclts()
        {
            return _treeLclts;
        }
        public static LimitedConcurrencyLevelTaskScheduler GetShareLclts()
        {
            return _shareLclts;
        }
        public static LimitedConcurrencyLevelTaskScheduler GetFileLclts()
        {
            return _fileLclts;
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
    }
}