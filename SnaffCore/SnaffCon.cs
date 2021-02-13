using Classifiers;
using SnaffCore.ActiveDirectory;
using SnaffCore.Concurrency;
using SnaffCore.Config;
using SnaffCore.ShareFind;
using SnaffCore.TreeWalk;
using SnaffCore.FileScan;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using static SnaffCore.Config.Options;
using Timer = System.Timers.Timer;

namespace SnaffCore
{
    public class SnaffCon
    {
        private bool AllTasksComplete { get; set; } = false;
        private BlockingMq Mq { get; set; }

        private static BlockingStaticTaskScheduler ShareTaskScheduler;
        private static BlockingStaticTaskScheduler TreeTaskScheduler;
        private static BlockingStaticTaskScheduler FileTaskScheduler;
        
        private static ShareFinder ShareFinder;
        private static TreeWalker TreeWalker;
        private static FileScanner FileScanner;

        public SnaffCon(Options options)
        {
            MyOptions = options;
            Mq = BlockingMq.GetMq();

            int shareThreads = MyOptions.ShareThreads;
            int treeThreads = MyOptions.TreeThreads;
            int fileThreads = MyOptions.FileThreads;

            ShareTaskScheduler = new BlockingStaticTaskScheduler(shareThreads, MyOptions.MaxShareQueue);
            TreeTaskScheduler = new BlockingStaticTaskScheduler(treeThreads, MyOptions.MaxTreeQueue);
            FileTaskScheduler = new BlockingStaticTaskScheduler(fileThreads, MyOptions.MaxFileQueue);

            FileScanner = new FileScanner();
            TreeWalker = new TreeWalker();
            ShareFinder = new ShareFinder();
        }

        public static ShareFinder GetShareFinder()
        {
            return ShareFinder;
        }
        public static TreeWalker GetTreeWalker()
        {
            return TreeWalker;
        }
        public static FileScanner GetFileScanner()
        {
            return FileScanner;
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
        public DateTime StartTime { get; set; }

        public void Execute()
        {
            StartTime = DateTime.Now;
            // This is the main execution thread.
            Timer statusUpdateTimer =
                new Timer(TimeSpan.FromMinutes(0.5)
                    .TotalMilliseconds)
                { AutoReset = true }; // Set the time (1 min in this case)
            statusUpdateTimer.Elapsed += TimedStatusUpdate;
            statusUpdateTimer.Start();

            // if we haven't been told what dir or computer to target, we're going to need to do share discovery. that means finding computers from the domain.
            if (MyOptions.PathTargets == null && MyOptions.ComputerTargets == null)
            {
                DomainDiscovery();
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

            SpinWait.SpinUntil(() => AllTasksComplete);

            StatusUpdate();
            DateTime finished = DateTime.Now;
            TimeSpan runSpan = finished.Subtract(StartTime);
            Mq.Info("Finished at " + finished.ToLocalTime());
            Mq.Info("Snafflin' took " + runSpan);
            Mq.Finish();
        }

        private void DomainDiscovery()
        {
            Mq.Info("Getting users and computers from AD.");
            // We do this single threaded cos it's fast and not easily divisible.
            AdData adData = new ActiveDirectory.AdData();
            List<string> targetComputers = adData.GetDomainComputers();
            if (targetComputers == null)
            {
                Mq.Error(
                    "Something fucked out finding stuff in the domain. You must be holding it wrong.");
                while (true)
                {
                    Mq.Terminate();
                }
            }
            string numTargetComputers = targetComputers.Count.ToString();
            Mq.Info("Got " + numTargetComputers + " computers from AD.");
            if (targetComputers.Count == 0)
            {
                Mq.Error("Didn't find any domain computers. Seems weird. Try pouring water on it.");
                while (true)
                {
                    Mq.Terminate();
                }
            }
            // Push list of fun users to Options
            if (MyOptions.DomainUserRules)
            {
                foreach (string user in adData.GetDomainUsers())
                {
                    MyOptions.DomainUsersToMatch.Add(user);
                }
                PrepDomainUserRules();
            }
            // immediately call ShareDisco which should handle the rest.
            ShareDiscovery(targetComputers.ToArray());
        }

        public void PrepDomainUserRules()
        {
            try
            {
                if (MyOptions.DomainUsersWordlistRules.Count >= 1)
                {
                    foreach (string ruleName in MyOptions.DomainUsersWordlistRules)
                    {
                        ClassifierRule configClassifierRule =
                            MyOptions.ClassifierRules.First(thing => thing.RuleName == ruleName);

                        foreach (string user in MyOptions.DomainUsersToMatch)
                        {
                            string pattern = "( |'|\")" + Regex.Escape(user) + "( |'|\")";
                            Regex regex = new Regex(pattern,
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                            configClassifierRule.Regexes.Add(regex);
                        }
                    }
                }
            }
            catch (Exception)
            {
                Mq.Error("Something went wrong adding domain users to rules.");
            }
        }

        private void ShareDiscovery(string[] computerTargets)
        {
            Mq.Info("Starting to find readable shares.");
            foreach (string computer in computerTargets)
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
                        Mq.Error("Exception in ShareFinder task for host " + computer);
                        Mq.Error(e.ToString());
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
                        TreeWalker.WalkTree(pathTarget);
                    }
                    catch (Exception e)
                    {
                        Mq.Error("Exception in TreeWalker task for path " + pathTarget);
                        Mq.Error(e.ToString());
                    }
                });
            }

            Mq.Info("Created all TreeWalker tasks.");
        }

        // This method is called every minute
        private void TimedStatusUpdate(object sender, ElapsedEventArgs e)
        {
            StatusUpdate();
        }

        private void StatusUpdate()
        {
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

            StringBuilder updateText = new StringBuilder();
            updateText.Append("\r\n" + "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~" + "\r\n");
            updateText.Append("Status Update: \r\n");

            // if all share tasks have finished, reduce max parallelism to 0 and reassign capacity to tree scheduler.
            if (ShareTaskScheduler.Done() && (shareTaskCounters.MaxParallelism >= 1))
            {
                int transferVal = ShareTaskScheduler.Scheduler._maxDegreeOfParallelism;
                ShareTaskScheduler.Scheduler._maxDegreeOfParallelism = 0;
                TreeTaskScheduler.Scheduler._maxDegreeOfParallelism = TreeTaskScheduler.Scheduler._maxDegreeOfParallelism + transferVal;
                updateText.Append("ShareScanner queue finished, rebalancing workload." + "\r\n");
            }

            // do the rebalancing
            if (fileTaskCounters.CurrentTasksQueued <= (MyOptions.MaxFileQueue / 20))
            {
                // but only if one side isn't already at minimum.
                if (FileTaskScheduler.Scheduler._maxDegreeOfParallelism > 1)
                {
                    updateText.Append("Insufficient FileScanner queue size, rebalancing workload." + "\r\n");
                    --FileTaskScheduler.Scheduler._maxDegreeOfParallelism;
                    ++TreeTaskScheduler.Scheduler._maxDegreeOfParallelism;
                }
            }
            if (fileTaskCounters.CurrentTasksQueued == MyOptions.MaxFileQueue)
            {
                if (TreeTaskScheduler.Scheduler._maxDegreeOfParallelism > 1)
                {
                    updateText.Append("Max FileScanner queue size reached, rebalancing workload." + "\r\n");
                    --TreeTaskScheduler.Scheduler._maxDegreeOfParallelism;
                    ++FileTaskScheduler.Scheduler._maxDegreeOfParallelism;
                }
            }
            updateText.Append("--------------------------------" + "\r\n");
            updateText.Append("Max ShareFinder Parallelism: " + ShareTaskScheduler.Scheduler._maxDegreeOfParallelism + "\r\n");
            updateText.Append("ShareFinder Tasks Completed: " + shareTaskCounters.CompletedTasks + "\r\n");
            updateText.Append("ShareFinder Tasks Remaining: " + shareTaskCounters.CurrentTasksRemaining + "\r\n");
            updateText.Append("ShareFinder Tasks Running: " + shareTaskCounters.CurrentTasksRunning + "\r\n");
            updateText.Append("--------------------------------" + "\r\n");
            updateText.Append("Max TreeWalker Parallelism: " + TreeTaskScheduler.Scheduler._maxDegreeOfParallelism + "\r\n");
            updateText.Append("TreeWalker Tasks Completed: " + treeTaskCounters.CompletedTasks + "\r\n");
            updateText.Append("TreeWalker Tasks Remaining: " + treeTaskCounters.CurrentTasksRemaining + "\r\n");
            updateText.Append("TreeWalker Tasks Running: " + treeTaskCounters.CurrentTasksRunning + "\r\n");
            updateText.Append("--------------------------------" + "\r\n");
            updateText.Append("Max FileScanner Parallelism: " + FileTaskScheduler.Scheduler._maxDegreeOfParallelism + "\r\n");
            updateText.Append("FileScanner Tasks Completed: " + fileTaskCounters.CompletedTasks + "\r\n");
            updateText.Append("FileScanner Tasks Remaining: " + fileTaskCounters.CurrentTasksRemaining + "\r\n");
            updateText.Append("FileScanner Tasks Running: " + fileTaskCounters.CurrentTasksRunning + "\r\n");
            updateText.Append("--------------------------------" + "\r\n");
            updateText.Append(memorynumber + " RAM in use." + "\r\n");
            DateTime now = DateTime.Now;
            TimeSpan runSpan = now.Subtract(StartTime);
            updateText.Append("Been Snafflin' for " + runSpan + " and we ain't done yet..." + "\r\n");
            updateText.Append("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~" + "\r\n" + "\r\n");


            Mq.Info(updateText.ToString());

            if (FileTaskScheduler.Done() && ShareTaskScheduler.Done() && TreeTaskScheduler.Done())
            {
                AllTasksComplete = true;
            }
        }

        private static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "kB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num) + suf[place];
        }
    }
}