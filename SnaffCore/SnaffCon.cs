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
using Timer = System.Timers.Timer;
using static SnaffCore.Config.Options;

namespace SnaffCore
{
    public class SnaffCon
    {
        private bool AllTasksComplete { get; set; } = false;
        private BlockingMq Mq { get; set; }
        private int CompletedTaskCounter { get; set; } = 0;

        public SnaffCon()
        {
            Mq = BlockingMq.GetMq();
            LimitedConcurrencyLevelTaskScheduler.CreateLCLTSes(MyOptions.MaxThreads);
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
            TaskFactory SharefinderTaskFactory = LimitedConcurrencyLevelTaskScheduler.GetShareTaskFactory();
            CancellationTokenSource SharefinderCts = LimitedConcurrencyLevelTaskScheduler.GetShareCts();

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
            TaskFactory SharescannerTaskFactory = LimitedConcurrencyLevelTaskScheduler.GetShareTaskFactory();
            CancellationTokenSource SharescannerCts = LimitedConcurrencyLevelTaskScheduler.GetShareCts();

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

        // This method is called every minute
        private void StatusUpdate(object sender, ElapsedEventArgs e)
        {
            string memorynumber;

            using (Process proc = Process.GetCurrentProcess())
            {
                long memorySize64 = proc.PrivateMemorySize64;
                memorynumber = BytesToString(memorySize64);
            }

            List<Task> SnafflerTasks = LimitedConcurrencyLevelTaskScheduler.GetSnafflerTaskList();

            int completedTasksCount = 0;
            int remainingTasksCount = 0;
            lock (SnafflerTasks)
            {
                // get the completed stuff from the last minute
                Task[] completedTasks = Array.FindAll(SnafflerTasks.ToArray(),
                    element => element.Status == TaskStatus.RanToCompletion);
                // count it
                completedTasksCount = completedTasks.Length;
                // remove them
                foreach (Task task in completedTasks)
                {
                    SnafflerTasks.Remove(task);
                }
                Task[] remainingTasks = Array.FindAll(SnafflerTasks.ToArray(),
                    element => element.Status != TaskStatus.RanToCompletion);
                remainingTasksCount = remainingTasks.Length;
            }
            // add the count of tasks that were completed since we last checked.
            CompletedTaskCounter = CompletedTaskCounter + completedTasksCount;

            var updateText = new StringBuilder("Status Update: \n");

            updateText.Append("Tasks Completed: " + completedTasksCount + "\n");
            updateText.Append("Tasks Remaining: " +
                              (remainingTasksCount) + "\n");
            updateText.Append(memorynumber + " RAM in use.");

            Mq.Info(updateText.ToString());

            if (remainingTasksCount == 0)
            {
                AllTasksComplete = true;
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
    }
}