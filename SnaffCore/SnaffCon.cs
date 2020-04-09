using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using SnaffCore.ComputerFind;
using SnaffCore.Concurrency;
using Classifiers;
using SnaffCore.ShareFind;
using SnaffCore.TreeWalk;
using Timer = System.Timers.Timer;

namespace SnaffCore
{
    public class SnaffCon
    {
        public SnaffCon()
        {
            Config.Config myConfig = Config.Config.GetConfig();
            Concurrency.LimitedConcurrencyLevelTaskScheduler.CreateLCLTSes(myConfig.Options.MaxThreads);
        }

        public void Execute()
        {
            // This is the main execution thread.
            BlockingMq Mq = BlockingMq.GetMq();
            Config.Config myConfig = Config.Config.GetConfig();
            TaskFactory SharefinderTaskFactory = LimitedConcurrencyLevelTaskScheduler.GetShareFinderTaskFactory();
            CancellationTokenSource SharefinderCts = LimitedConcurrencyLevelTaskScheduler.GetShareFinderCts();

            TaskFactory treeWalkerTaskFactory = LimitedConcurrencyLevelTaskScheduler.GetTreeWalkerTaskFactory();
            CancellationTokenSource treeWalkerCts = LimitedConcurrencyLevelTaskScheduler.GetTreeWalkerCts();

            List<string> targetComputers = new List<string>();
            ConcurrentBag<ShareResult> foundShares = new ConcurrentBag<ShareResult>();

            Timer statusUpdateTimer =
                new Timer(TimeSpan.FromMinutes(1)
                    .TotalMilliseconds) {AutoReset = true}; // Set the time (1 min in this case)
            statusUpdateTimer.Elapsed += StatusUpdate;
            statusUpdateTimer.Start();

            // if we haven't been told what dir or computer to target, we're going to need to do share discovery. that means finding computers from the domain.
            if (myConfig.Options.PathTargets == null && myConfig.Options.ComputerTargets == null)
            {
                ComputerDiscovery();
            }
            // if we've been told what computers to hit...
            else if (myConfig.Options.ComputerTargets != null)
            {
                ShareDiscovery(myConfig.Options.ComputerTargets);
            }
            // otherwise we should have a set of path targets...
            else if (myConfig.Options.PathTargets != null)
            {
                FileDiscovery(myConfig.Options.PathTargets);
            }
            // but if that hasn't been done, something has gone wrong.
            else
            {
                Mq.Error("OctoParrot says: AWK! I SHOULDN'T BE!");
            }

            // TODO (NOT THIS)
            while (true)
            {
                string thing = null;
                // lol whaaaayy
            }

            Mq.Info("Finished!");
            Console.ResetColor();
            Environment.Exit(0);
        }

        private void ComputerDiscovery()
        {
            BlockingMq Mq = BlockingMq.GetMq();
            Config.Config myConfig = Config.Config.GetConfig();

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
            BlockingMq Mq = BlockingMq.GetMq();
            Config.Config myConfig = Config.Config.GetConfig();
            TaskFactory SharefinderTaskFactory = LimitedConcurrencyLevelTaskScheduler.GetShareFinderTaskFactory();
            CancellationTokenSource SharefinderCts = LimitedConcurrencyLevelTaskScheduler.GetShareFinderCts();

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
            BlockingMq Mq = BlockingMq.GetMq();
            Config.Config myConfig = Config.Config.GetConfig();
            TaskFactory SharescannerTaskFactory = LimitedConcurrencyLevelTaskScheduler.GetTreeWalkerTaskFactory();
            CancellationTokenSource SharescannerCts = LimitedConcurrencyLevelTaskScheduler.GetTreeWalkerCts();

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
            BlockingMq Mq = BlockingMq.GetMq();
            /*
            var totalShareFinderTasksCount = ShareFinderTasks.Count;
            var totalShareScannerTasksCount = TreeWalkerTasks.Count;

            var completedShareFinderTasks = Array.FindAll(ShareFinderTasks.ToArray(),
                element => element.Status == TaskStatus.RanToCompletion);
            var completedShareFinderTasksCount = completedShareFinderTasks.Length;

            var completedShareScannerTasks = Array.FindAll(TreeWalkerTasks.ToArray(),
                element => element.Status == TaskStatus.RanToCompletion);
            var completedShareScannerTasksCount = completedShareScannerTasks.Length;

            var updateText = new StringBuilder("Status Update: \n");

            updateText.Append("Sharescanner Tasks Completed: " + completedShareScannerTasksCount + "\n");
            updateText.Append("Sharescanner Tasks Remaining: " +
                              (totalShareScannerTasksCount - completedShareScannerTasksCount) + "\n");
            updateText.Append("TreeWalker Tasks Completed: " + completedShareFinderTasksCount + "\n");
            updateText.Append("TreeWalker Tasks Remaining: " +
                              (totalShareFinderTasksCount - completedShareFinderTasksCount) + "\n");

            Mq.Info(updateText.ToString());
            */
            Mq.Info("Status Updates are broken still you big goober!");
        }
    }
}