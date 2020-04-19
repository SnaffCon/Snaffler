using System;
using System.Numerics;
using System.Collections.Generic;
using System.Diagnostics;
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

        //private object StatusObjectLocker = new object();
        //
        //private BigInteger CompletedFileTaskCounter { get; set; } = 0;
        //private BigInteger RemainingFileTaskCounter { get; set; } = 0;
        //private BigInteger CompletedShareTaskCounter { get; set; } = 0;
        //private BigInteger RemainingShareTaskCounter { get; set; } = 0;
        //private BigInteger CompletedTreeTaskCounter { get; set; } = 0;
        //private BigInteger RemainingTreeTaskCounter { get; set; } = 0;
        private static BlockingStaticTaskScheduler ShareTaskScheduler;
        private static BlockingStaticTaskScheduler TreeTaskScheduler;
        private static BlockingStaticTaskScheduler FileTaskScheduler;

        public SnaffCon(Options options)
        {
            MyOptions = options;
            Mq = BlockingMq.GetMq();

            //int threads = MyOptions.MaxThreads;
            //int threads = 30;
            int shareThreads = MyOptions.ShareThreads;
            int treeThreads = MyOptions.TreeThreads;
            int fileThreads = MyOptions.FileThreads;

            ShareTaskScheduler = new BlockingStaticTaskScheduler(shareThreads, MyOptions.MaxShareQueue);
            TreeTaskScheduler = new BlockingStaticTaskScheduler(treeThreads, MyOptions.MaxTreeQueue);
            FileTaskScheduler = new BlockingStaticTaskScheduler(fileThreads, MyOptions.MaxFileQueue);
        }

        public static BlockingStaticTaskScheduler GetShareTaskScheduler()
        {
            return ShareTaskScheduler;
        }
        public static BlockingStaticTaskScheduler GetTreeTaskScheduler()
        {
            return TreeTaskScheduler;
        }
        public static BlockingStaticTaskScheduler GetFileTaskScheduler()
        {
            return FileTaskScheduler;
        }

        public void Execute()
        {
            // This is the main execution thread.
            Timer statusUpdateTimer =
                new Timer(TimeSpan.FromMinutes(1)
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
            Mq.Info("Starting to find readable shares.");
            foreach (var computer in computerTargets)
            {
                // ShareFinder Task Creation - this kicks off the rest of the flow
                Mq.Info("Creating a sharefinder task for " + computer);
                ShareTaskScheduler.New(() =>
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
                });
            }
            Mq.Info("Created all sharefinder tasks.");
        }

        private void FileDiscovery(string[] pathTargets)
        {
            foreach (string pathTarget in pathTargets)
            {
                // ShareScanner Task Creation - this kicks off the rest of the flow
                Mq.Info("Creating a TreeWalker task for " + pathTarget);
                TreeTaskScheduler.New(() =>
                {
                    try
                    {
                        TreeWalker treeWalker = new TreeWalker(pathTarget);
                    }
                    catch (Exception e)
                    {
                        Mq.Trace(e.ToString());
                    }
                });
            }

            Mq.Info("Created all TreeWalker tasks.");
        }

        // This method is called every minute
        private void StatusUpdate(object sender, ElapsedEventArgs e)
        {
            //lock (StatusObjectLocker)
            //{
                // get memory usage for status update
                string memorynumber;
                using (Process proc = Process.GetCurrentProcess())
                {
                    long memorySize64 = proc.PrivateMemorySize64;
                    memorynumber = BytesToString(memorySize64);
                }

                TaskCounters shareTaskCounters = ShareTaskScheduler.Scheduler.GetTaskCounters();
                TaskCounters treeTaskCounters = TreeTaskScheduler.Scheduler.GetTaskCounters();
                TaskCounters fileTaskCounters = FileTaskScheduler.Scheduler.GetTaskCounters();

                var updateText = new StringBuilder("Status Update: \n");
                updateText.Append("ShareFinder Tasks Completed: " + shareTaskCounters.CompletedTasks + "\n");
                updateText.Append("ShareFinder Tasks Remaining: " + shareTaskCounters.CurrentTasksRemaining + "\n");
                updateText.Append("ShareFinder Tasks Running: " + shareTaskCounters.CurrentTasksRunning + "\n");
                updateText.Append("TreeWalker Tasks Completed: " + treeTaskCounters.CompletedTasks + "\n");
                updateText.Append("TreeWalker Tasks Remaining: " + treeTaskCounters.CurrentTasksRemaining + "\n");
                updateText.Append("TreeWalker Tasks Running: " + treeTaskCounters.CurrentTasksRunning + "\n");
                updateText.Append("FileScanner Tasks Completed: " + fileTaskCounters.CompletedTasks + "\n");
                updateText.Append("FileScanner Tasks Remaining: " + fileTaskCounters.CurrentTasksRemaining + "\n");
                updateText.Append("FileScanner Tasks Running: " + fileTaskCounters.CurrentTasksRunning + "\n");
                updateText.Append(memorynumber + " RAM in use.");

                Mq.Info(updateText.ToString());

                if (FileTaskScheduler.Done() && ShareTaskScheduler.Done() && TreeTaskScheduler.Done())
                {
                    AllTasksComplete = true;
                }
            //}
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
    }
}