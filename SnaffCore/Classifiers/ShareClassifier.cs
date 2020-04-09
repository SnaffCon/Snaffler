using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SnaffCore.Concurrency;
using SnaffCore.TreeWalk;
using Config = SnaffCore.Config.Config;

namespace Classifiers
{
    public partial class Classifier
    {
        public void ClassifyShare(string share)
        {
            // TODO add a special case to dedupe sysvol/netlogon scanning
            TaskFactory treeWalkerTaskFactory = LimitedConcurrencyLevelTaskScheduler.GetTreeWalkerTaskFactory();
            CancellationTokenSource treeWalkerCts = LimitedConcurrencyLevelTaskScheduler.GetTreeWalkerCts();
            BlockingMq Mq = BlockingMq.GetMq();
            Config myConfig = Config.GetConfig();
            // check if it matches
            if (SimpleMatch(share))
            {
                // if it does, see what we're gonna do with it
                switch (MatchAction)
                {
                    case MatchAction.Discard:
                        return;
                    case MatchAction.Snaffle:
                        if (IsShareReadable(share))
                        {
                            ShareResult shareResult = new ShareResult()
                            {
                                Listable = true,
                                SharePath = share
                            };
                            Mq.ShareResult(shareResult);
                        }
                        return;
                    default:
                        Mq.Error("You've got a misconfigured share classifier named " + this.ClassifierName + ".");
                        return;
                }

                
            }
            // by default all shares should go on to TreeWalker
            // send them to TreeWalker
            if (IsShareReadable(share))
            {
                ShareResult shareResult = new ShareResult()
                {
                    Listable = true,
                    SharePath = share
                };
                Mq.ShareResult(shareResult);

                Mq.Info("Creating a TreeWalker task for " + shareResult.SharePath);
                var t = treeWalkerTaskFactory.StartNew(() =>
                {
                    try
                    {
                        new TreeWalker(shareResult.SharePath);
                    }
                    catch (Exception e)
                    {
                        Mq.Trace(e.ToString());
                    }
                }, treeWalkerCts.Token);
            }
        }

        internal bool IsShareReadable(string share)
        {
            BlockingMq Mq = BlockingMq.GetMq();
            try
            {
                string[] files = Directory.GetFiles(share);
                return true;
            }
            catch (Exception e)
            {
                Mq.Trace(e.ToString());
            }
            return false;
        }
    }

    public class ShareResult
    {
        public bool Snaffle { get; set; }
        public bool ScanShare { get; set; }
        public string SharePath { get; set; }
        public bool Listable { get; set; }
        public bool IsAdminShare { get; set; }
    }
}