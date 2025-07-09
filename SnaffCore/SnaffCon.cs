using SnaffCore.Classifiers;
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
using System.Net;
using System.Runtime.InteropServices;

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

        private AdData _adData = AdData.AdDataInstance;

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
            bool impersonateResult = Impersonator.StartImpersonating();
            if (!impersonateResult)
            {
                int errorCode = Marshal.GetLastWin32Error();
                Mq.Error($"[Error Code {errorCode}] Failed to impersonate {Impersonator.GetUsername()}.");
            }

            StartTime = DateTime.Now;
            // This is the main execution thread.
            Timer statusUpdateTimer =
                new Timer(TimeSpan.FromMinutes(MyOptions.TimeOut)
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
            if (MyOptions.PathTargets.Count != 0 && (MyOptions.DfsShareDiscovery || MyOptions.DfsOnly))
            {
                DomainDfsDiscovery();
            }

            if (MyOptions.PathTargets.Count == 0 && MyOptions.ComputerTargets == null)
            {
                if (MyOptions.DfsSharesDict.Count == 0)
                {
                    Mq.Info("Invoking DFS Discovery because no ComputerTargets or PathTargets were specified");
                    DomainDfsDiscovery();
                }

                if (!MyOptions.DfsOnly)
                {
                    Mq.Info("Invoking full domain computer discovery.");
                    DomainTargetDiscovery();
                }
                else
                {
                    Mq.Info("Skipping domain computer discovery.");
                    foreach (string share in MyOptions.DfsSharesDict.Keys)
                    {
                        if (!MyOptions.PathTargets.Contains(share))
                        {
                            MyOptions.PathTargets.Add(share);
                        }
                    }
                    Mq.Info("Starting TreeWalker tasks on DFS shares.");
                    FileDiscovery(MyOptions.PathTargets.ToArray());
                }
            }
            // otherwise we should have a set of path targets...
            else if (MyOptions.PathTargets.Count != 0)
            {
                FileDiscovery(MyOptions.PathTargets.ToArray());
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

            Impersonator.StopImpersonating();
        }

        private void DomainDfsDiscovery()
        {
            Dictionary<string, string> dfsSharesDict = null;

            Mq.Info("Getting DFS paths from AD.");

            _adData.SetDfsPaths();
            dfsSharesDict = _adData.GetDfsSharesDict();

            // if we found some actual dfsshares
            if (dfsSharesDict.Count >= 1)
            {
                MyOptions.DfsSharesDict = dfsSharesDict;
                MyOptions.DfsNamespacePaths = _adData.GetDfsNamespacePaths();
            }
        }

        private void DomainTargetDiscovery()
        {
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

                _adData.SetDomainComputers(MyOptions.ComputerTargetsLdapFilter);

                targetComputers = _adData.GetDomainComputers();
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

            _adData.SetDomainUsers();

            foreach (string user in _adData.GetDomainUsers())
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
                            if (user.Length < MyOptions.DomainUserMinLen)
                            {
                                Mq.Trace(String.Format("Skipping regex for \"{0}\".  Shorter than minimum chars: {1}", user, MyOptions.DomainUserMinLen));
                                continue;
                            }

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
                if (CheckExclusions(computer))
                {
                    // skip any that are in the exclusion list
                    continue;
                }
                // Perform reverse lookup if the computer is an IP address
                var computerName = "";
                if (isIP(computer))
                {
                    try
                    {
                        Mq.Trace("Performing reverse lookup for " + computer);
                        IPHostEntry result = Dns.GetHostEntry(computer);
                        computerName = result.HostName;
                        Mq.Trace("Got DNSName " + computerName + " for " + computer);
                    }
                    catch (Exception e)
                    {
                        Mq.Degub(e.Message);
                        // Keep IP if Reverse Lookup fails
                        computerName = computer;
                    }
                }
                else
                {
                    // Use the provided computer name if it's not an IP address
                    computerName = computer;
                }
                // ShareFinder Task Creation - this kicks off the rest of the flow
                Mq.Trace("Creating a ShareFinder task for " + computerName);
                ShareTaskScheduler.New(() =>
                {
                    try
                    {
                        ShareFinder shareFinder = new ShareFinder();
                        shareFinder.GetComputerShares(computerName);
                    }
                    catch (Exception e)
                    {
                        Mq.Error("Exception in ShareFinder task for host " + computerName);
                        Mq.Error(e.ToString());
                    }
                });
            }
            Mq.Info("Created all sharefinder tasks.");
        }

        public static bool isIP(string host)
        {
            IPAddress ip;
            return IPAddress.TryParse(host, out ip);
        }

        private bool CheckExclusions(string computer)
        {
            // check if it's an IP already
            if (isIP(computer))
            {
                // check if it's in the exclusions list
                if (MyOptions.ComputerExclusions.Contains(computer))
                {
                    // if so, skip it
                    Mq.Degub("Excluded " + computer);
                    return true;
                }
            }
            else { 
                try
                {
                    // resolve it
                    IPHostEntry result = Dns.GetHostEntry(computer);
                    // handle multiple IPs in response
                    foreach (IPAddress ipAddress in result.AddressList)
                    {
                        // if any of them is in the exclusion list
                        if (MyOptions.ComputerExclusions.Contains(ipAddress.ToString()))
                        {
                            Mq.Degub("Excluded " + computer + " at " + ipAddress);

                            // exclude it
                            return true;
                        };
                    }
                }
                catch (Exception ex)
                {
                    // fail safe
                    Mq.Degub(ex.Message);
                    //Console.WriteLine(ex.Message);
                    Mq.Degub("Excluded " + computer);
                    return true; ;
                }
            }
            Mq.Degub("Included " + computer);

            return false;
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
