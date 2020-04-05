using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using SnaffCore.ComputerFind;
using SnaffCore.Concurrency;
using SnaffCore.ShareFind;
using SnaffCore.ShareScan;
using Timer = System.Timers.Timer;

namespace SnaffCore
{
    public class SnaffCon
    {
        private Config.Config Config { get; set; }
        private LimitedConcurrencyLevelTaskScheduler SharefinderLcts { get; set; }
        private TaskFactory SharefinderTaskFactory { get; set; }
        private CancellationTokenSource SharefinderCts { get; set; }
        private List<Task> SharefinderTasks { get; set; } = new List<Task>();
        private LimitedConcurrencyLevelTaskScheduler SharescannerLcts { get; set; }
        private TaskFactory SharescannerTaskFactory { get; set; }
        private CancellationTokenSource SharescannerCts { get; set; }
        private List<Task> SharescannerTasks { get; set; } = new List<Task>();
        private bool SysvolTaskCreated { get; set; }
        private bool NetlogonTaskCreated { get; set; }

        public SnaffCon(Config.Config conf)
        {
            Config = conf;
            SharefinderLcts = new LimitedConcurrencyLevelTaskScheduler(Config.MaxThreads);
            SharefinderTaskFactory = new TaskFactory(SharefinderLcts);
            SharefinderCts = new CancellationTokenSource();
            SharescannerLcts = new LimitedConcurrencyLevelTaskScheduler(Config.MaxThreads);
            SharescannerTaskFactory = new TaskFactory(SharescannerLcts);
            SharescannerCts = new CancellationTokenSource();
        }

        public void Execute()
        {
            var targetComputers = new List<string>();
            var foundShares = new ConcurrentBag<string>();

            var statusUpdateTimer =
                new Timer(TimeSpan.FromMinutes(1)
                    .TotalMilliseconds) {AutoReset = true}; // Set the time (5 mins in this case)
            statusUpdateTimer.Elapsed += StatusUpdate;
            statusUpdateTimer.Start();

            if (Config.Options.DirTarget == null)
            {
                Config.Mq.Info("Getting computers from AD.");
                // We do this single threaded cos it's fast and not easily divisible.
                var activeDirectory = new ActiveDirectory(Config);
                targetComputers = activeDirectory.DomainComputers;
                if (targetComputers == null)
                {
                    Config.Mq.Error(
                        "Something fucked out finding the computers in the domain. You must be holding it wrong.");
                    while (true)
                    {
                        Config.Mq.Terminate();
                    }
                }

                var numTargetComputers = targetComputers.Count.ToString();
                Config.Mq.Info("Got " + numTargetComputers + " computers from AD.");
                if (targetComputers.Count == 0)
                {
                    Config.Mq.Error("Didn't find any domain computers. Seems weird. Try pouring water on it.");
                    while (true)
                    {
                        Config.Mq.Terminate();
                    }
                }
            }
            else
            {
                foundShares.Add(Config.Options.DirTarget);
            }

            if (Config.Options.ShareFinderEnabled)
            {
                // TODO: change this to use a method that pulls data from the options classifiers
                foundShares = ShareFindingMagic(targetComputers);
            }


            if (Config.Options.ShareScanEnabled)
            {
                var shareFinderTasksDone = false;
                Config.Mq.Info("Starting to search shares for files.");
                // keep going until all sharefinder tasks are completed or faulted, and there's no shares left to start scanner tasks for
                while (shareFinderTasksDone == false || !foundShares.IsEmpty)
                {
                    // check if all the shareFinder Tasks are done
                    var completedShareFinderTasks = Array.FindAll(SharefinderTasks.ToArray(),
                        element => element.Status == TaskStatus.RanToCompletion);
                    var faultedShareFinderTasks = Array.FindAll(SharefinderTasks.ToArray(),
                        element => element.Status == TaskStatus.Faulted);
                    if ((completedShareFinderTasks.Length + faultedShareFinderTasks.Length) ==
                        SharefinderTasks.Count)
                    {
                        // update the completion status.
                        shareFinderTasksDone = true;
                        Config.Mq.Info("All Sharefinder Tasks completed.");
                    }

                    //pull shares out of the result bag and make scanner tasks for them.
                    while (foundShares.TryTake(out var share))
                    {
                        if (!String.IsNullOrWhiteSpace(share))
                        {
                            // skip ipc$ and print$ every time
                            // TODO: remove this skip logic once the patterns from the config file have been applied when enumerating shares
                            if ((share.ToLower().EndsWith("ipc$")) || (share.ToLower().EndsWith("print$")))
                            {
                                continue;
                            }

                            if (share.ToLower().EndsWith("sysvol"))
                            {
                                if (SysvolTaskCreated)
                                {
                                    continue;
                                }

                                SysvolTaskCreated = true;
                            }
                            else if (share.ToLower().EndsWith("netlogon"))
                            {
                                if (NetlogonTaskCreated)
                                {
                                    continue;
                                }

                                NetlogonTaskCreated = true;
                            }

                            // check if it's an admin share
                            var isCDollarShare = share.EndsWith("C$");
                            // put a result on the queue
                            Config.Mq.ShareResult(new ShareFinder.ShareResult {IsAdminShare = isCDollarShare, Listable = true, SharePath = share});
                            // bail out if we're not scanning admin shares
                            if (isCDollarShare && !Config.Options.ScanCDollarShares)
                            {
                                continue;
                            }

                            // otherwise create a TreeWalker task
                            Config.Mq.Info("Creating ShareScanner for:" + share);
                            var t = SharescannerTaskFactory.StartNew(() =>
                            {
                                try
                                {
                                    var treeWalker = new TreeWalker(Config, share);
                                }
                                catch (Exception e)
                                {
                                    Config.Mq.Trace(e.ToString());
                                }
                            }, SharescannerCts.Token);
                            SharescannerTasks.Add(t);
                        }
                    }
                }

                var shareScannerTasksDone = false;

                while (!shareScannerTasksDone)
                {
                    var completedShareScannerTasks = Array.FindAll(SharescannerTasks.ToArray(),
                        element => element.Status == TaskStatus.RanToCompletion);
                    var faultedShareScannerTasks = Array.FindAll(SharescannerTasks.ToArray(),
                        element => element.Status == TaskStatus.Faulted);
                    if ((completedShareScannerTasks.Length + faultedShareScannerTasks.Length) ==
                        SharescannerTasks.Count)
                    {
                        shareScannerTasksDone = true;
                        Config.Mq.Info("All ShareScanner tasks finished!");
                    }
                }
            }

            Config.Mq.Info("Finished!");
            Console.ResetColor();
            Environment.Exit(0);
            // This is the main execution thread.
        }

        private ConcurrentBag<string> ShareFindingMagic(List<string> targetComputers)
        {
            ConcurrentBag<string> foundShares = new ConcurrentBag<string>();
            Config.Mq.Info("Starting to find readable shares.");
            foreach (var computer in targetComputers)
            {
                Config.Mq.Info("Creating a sharefinder task for " + computer);
                var t = SharefinderTaskFactory.StartNew(() =>
                {
                    try
                    {
                        var shareFinder = new ShareFinder();
                        var taskFoundShares = shareFinder.GetReadableShares(computer, Config);
                        if (taskFoundShares.Count > 0)
                        {
                            foreach (var taskFoundShare in taskFoundShares)
                            {
                                if (!String.IsNullOrWhiteSpace(taskFoundShare))
                                {
                                    foundShares.Add(taskFoundShare);
                                }
                            }
                        }
                        else
                        {
                            Config.Mq.Info(computer + " had no shares on it");
                        }
                    }
                    catch (Exception e)
                    {
                        Config.Mq.Trace(e.ToString());
                    }
                }, SharefinderCts.Token);
                SharefinderTasks.Add(t);
            }

            Config.Mq.Info("Created all " + SharefinderTasks.Count + " sharefinder tasks.");
            return foundShares;
        }

        // This method is called every minute
        private void StatusUpdate(object sender, ElapsedEventArgs e)
        {
            var totalShareFinderTasksCount = SharefinderTasks.Count;
            var totalShareScannerTasksCount = SharescannerTasks.Count;
            var completedShareFinderTasks = Array.FindAll(SharefinderTasks.ToArray(),
                element => element.Status == TaskStatus.RanToCompletion);
            var completedShareFinderTasksCount = completedShareFinderTasks.Length;

            var completedShareScannerTasks = Array.FindAll(SharescannerTasks.ToArray(),
                element => element.Status == TaskStatus.RanToCompletion);
            var completedShareScannerTasksCount = completedShareScannerTasks.Length;

            var updateText = new StringBuilder("Status Update: \n");

            updateText.Append("Sharescanner Tasks Completed: " + completedShareScannerTasksCount + "\n");
            updateText.Append("Sharescanner Tasks Remaining: " +
                              (totalShareScannerTasksCount - completedShareScannerTasksCount) + "\n");
            updateText.Append("Sharefinder Tasks Completed: " + completedShareFinderTasksCount + "\n");
            updateText.Append("Sharefinder Tasks Remaining: " +
                              (totalShareFinderTasksCount - completedShareFinderTasksCount) + "\n");

            Config.Mq.Info(updateText.ToString());
        }
    }
}