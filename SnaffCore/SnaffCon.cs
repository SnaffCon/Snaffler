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
        private readonly EventWaitHandle waitHandle = new AutoResetEvent(false);

        private BlockingMq Mq { get; set; }

        private static BlockingStaticTaskScheduler ShareTaskScheduler;
        private static BlockingStaticTaskScheduler TreeTaskScheduler;
        private static BlockingStaticTaskScheduler FileTaskScheduler;
        
        private static ShareFinder ShareFinder;
        private static TreeWalker TreeWalker;
        private static FileScanner FileScanner;

        private DateTime StartTime { get; set; }

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

        public void Execute()
        {
            StartTime = DateTime.Now;
            // This is the main execution thread.
            Timer statusUpdateTimer =
                new Timer(TimeSpan.FromMinutes(1)
                    .TotalMilliseconds)
                { AutoReset = true }; // Set the time (1 min in this case)
            statusUpdateTimer.Elapsed += TimedStatusUpdate;
            statusUpdateTimer.Start();


            // If we want to hunt for user IDs, we need data from the running user's domain.
            // Future - walk trusts
            if ( MyOptions.DomainUserRules)
            {
                DomainUserDiscovery();
            }

            // Explicit folder setting overrides DFS
            if (MyOptions.PathTargets == null && (MyOptions.DfsShareDiscovery || MyOptions.DfsOnly))
            {
                DomainDfsDiscovery();
            }

            if (MyOptions.PathTargets == null && MyOptions.ComputerTargets == null)
            {
                if(MyOptions.DfsSharesDict == null)
                {
                    Mq.Info("Invoking DFS Discovery because ComputerTargets and PathTargets were being discovered");
                    DomainDfsDiscovery();
                }
                DomainTargetDiscovery();
            }
            // otherwise we should have a set of path targets...
            else if (MyOptions.PathTargets != null)
            {
                FileDiscovery(MyOptions.PathTargets);
            }
            // or we've been told what computers to hit...
            else if (MyOptions.ComputerTargets != null)
            {
                ShareDiscovery(MyOptions.ComputerTargets);
            }
            // but if that hasn't been done, something has gone wrong.
            else
            {
                Mq.Error("OctoParrot says: AWK! I SHOULDN'T BE!");
            }

            waitHandle.WaitOne();

            StatusUpdate();
            DateTime finished = DateTime.Now;
            TimeSpan runSpan = finished.Subtract(StartTime);
            Mq.Info("Finished at " + finished.ToLocalTime());
            Mq.Info("Snafflin' took " + runSpan);
            Mq.Finish();
        }

        private void DomainDfsDiscovery()
        {
            Dictionary<string, string> dfsSharesDict = null;
            AdData adData = null;

            Mq.Info("Getting DFS paths from AD.");

            adData = new AdData();

            adData.SetDfsPaths();
            dfsSharesDict = adData.GetDfsSharesDict();

            // if we found some actual dfsshares
            if (dfsSharesDict.Count >= 1)
            {
                MyOptions.DfsSharesDict = dfsSharesDict;
                MyOptions.DfsNamespacePaths = adData.GetDfsNamespacePaths();
            }
        }

        private void DomainTargetDiscovery()
        {
            AdData adData = null;
            List<string> targetComputers;


            // Give preference to explicit targets in the options file over LDAP computer discovery
            if (MyOptions.ComputerTargets != null)  
            {
                Mq.Info("Using computer list from user-specified options.");

                targetComputers = new List<string>();
                foreach (string t in MyOptions.ComputerTargets)
                {
                    targetComputers.Add(t);
                }
                Mq.Info(string.Format("Took {0} computers specified in options file.", targetComputers.Count));
            }
            else
            {
                // We do this single threaded cos it's fast and not easily divisible.
                Mq.Info("Getting computers from AD.");

                // The AdData class set/get semantics have gotten wonky here.  Leaving as-is to minimize breakage/changes, but needs another look.
                // initialize it if we didn't already
                if(adData == null)
                {
                    adData = new AdData();
                }
                adData.SetDomainComputers(MyOptions.ComputerTargetsLdapFilter);

                targetComputers = adData.GetDomainComputers();
                Mq.Info(string.Format("Got {0} computers from AD.", targetComputers.Count));

                // if we're only scanning DFS shares then we can skip the SMB sharefinder and work from the list in AD, then jump to FileDiscovery().
                if (MyOptions.DfsOnly)
                {
                    FileDiscovery(MyOptions.DfsNamespacePaths.ToArray());
                }
            }

            if (targetComputers == null && MyOptions.DfsNamespacePaths == null)
            {
                Mq.Error(
                    "Something fucked out finding stuff in the domain. You must be holding it wrong.");
                while (true)
                {
                    Mq.Terminate();
                }
            }

            if (targetComputers.Count == 0 && MyOptions.DfsNamespacePaths.Count == 0)
            {
                Mq.Error("Didn't find any domain computers. Seems weird. Try pouring water on it.");
                while (true)
                {
                    Mq.Terminate();
                }
            }

            // call ShareDisco which should handle the rest.
            ShareDiscovery(targetComputers.ToArray());
            // ShareDiscovery(targetComputers.ToArray(), dfsShares);
        }

        private void DomainUserDiscovery()
        {
            Mq.Info("Getting interesting users from AD.");
            // We do this single threaded cos it's fast and not easily divisible.

            // The AdData class set/get semantics have gotten wonky here.  Leaving as-is to minimize breakage/changes, but needs another look.
            AdData adData = new AdData();
            adData.SetDomainUsers();

            foreach (string user in adData.GetDomainUsers())
            {
                MyOptions.DomainUsersToMatch.Add(user);
            }

            // build the regexes for use in the file scans
            PrepDomainUserRules();
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
                            // Use the null character to match begin and end of line
                            string pattern = "(| |'|\")" + Regex.Escape(user) + "(| |'|\")";
                            Regex regex = new Regex(pattern,
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
                            configClassifierRule.Regexes.Add(regex);
                            Mq.Trace(String.Format("Adding regex {0} to rule {1}", regex, ruleName));
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
            Mq.Info("Starting to look for readable shares...");
            foreach (string computer in computerTargets)
            {
                // ShareFinder Task Creation - this kicks off the rest of the flow
                Mq.Trace("Creating a ShareFinder task for " + computer);
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
                // TreeWalker Task Creation - this kicks off the rest of the flow
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

            StringBuilder updateText = new StringBuilder("Status Update: \n");
            updateText.Append("ShareFinder Tasks Completed: " + shareTaskCounters.CompletedTasks + "\n");
            updateText.Append("ShareFinder Tasks Remaining: " + shareTaskCounters.CurrentTasksRemaining + "\n");
            updateText.Append("ShareFinder Tasks Running: " + shareTaskCounters.CurrentTasksRunning + "\n");
            updateText.Append("TreeWalker Tasks Completed: " + treeTaskCounters.CompletedTasks + "\n");
            updateText.Append("TreeWalker Tasks Remaining: " + treeTaskCounters.CurrentTasksRemaining + "\n");
            updateText.Append("TreeWalker Tasks Running: " + treeTaskCounters.CurrentTasksRunning + "\n");
            updateText.Append("FileScanner Tasks Completed: " + fileTaskCounters.CompletedTasks + "\n");
            updateText.Append("FileScanner Tasks Remaining: " + fileTaskCounters.CurrentTasksRemaining + "\n");
            updateText.Append("FileScanner Tasks Running: " + fileTaskCounters.CurrentTasksRunning + "\n");
            updateText.Append(memorynumber + " RAM in use." + "\n");
            updateText.Append("\n");

            // if all share tasks have finished, reduce max parallelism to 0 and reassign capacity to file scheduler.
            if (ShareTaskScheduler.Done() && (shareTaskCounters.MaxParallelism >= 1))
            {
                // get the current number of sharetask threads
                int transferVal = shareTaskCounters.MaxParallelism;
                // set it to zero
                ShareTaskScheduler.Scheduler._maxDegreeOfParallelism = 0;
                // add 1 to the other
                FileTaskScheduler.Scheduler._maxDegreeOfParallelism = FileTaskScheduler.Scheduler._maxDegreeOfParallelism + transferVal;
                updateText.Append("ShareScanner queue finished, rebalancing workload." + "\n");
            }

            // do other rebalancing

            if (fileTaskCounters.CurrentTasksQueued <= (MyOptions.MaxFileQueue / 20))
            {
                // but only if one side isn't already at minimum
                if (FileTaskScheduler.Scheduler._maxDegreeOfParallelism > 1)
                {
                    updateText.Append("Insufficient FileScanner queue size, rebalancing workload." + "\n");
                    --FileTaskScheduler.Scheduler._maxDegreeOfParallelism;
                    ++TreeTaskScheduler.Scheduler._maxDegreeOfParallelism;
                }
            }
            if (fileTaskCounters.CurrentTasksQueued == MyOptions.MaxFileQueue)
            {
                if (TreeTaskScheduler.Scheduler._maxDegreeOfParallelism > 1)
                {
                    updateText.Append("Max FileScanner queue size reached, rebalancing workload." + "\n");
                    ++FileTaskScheduler.Scheduler._maxDegreeOfParallelism;
                    --TreeTaskScheduler.Scheduler._maxDegreeOfParallelism;
                }
            }

            updateText.Append("Max ShareFinder Threads: " + ShareTaskScheduler.Scheduler._maxDegreeOfParallelism + "\n");
            updateText.Append("Max TreeWalker Threads: " + TreeTaskScheduler.Scheduler._maxDegreeOfParallelism + "\n");
            updateText.Append("Max FileScanner Threads: " + FileTaskScheduler.Scheduler._maxDegreeOfParallelism + "\n");

            DateTime now = DateTime.Now;
            TimeSpan runSpan = now.Subtract(StartTime);

            updateText.Append("Been Snafflin' for " + runSpan + " and we ain't done yet..." + "\n");

            Mq.Info(updateText.ToString());

            if (FileTaskScheduler.Done() && ShareTaskScheduler.Done() && TreeTaskScheduler.Done())
            {
                waitHandle.Set();
            }
            //}
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
