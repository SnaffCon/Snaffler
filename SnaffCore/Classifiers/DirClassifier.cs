using SnaffCore.Concurrency;
using SnaffCore.ShareScan;
using Config = SnaffCore.Config.Config;

namespace Classifiers
{
    public partial class Classifier
    {
        public TreeWalker.DirResult ClassifyDir(string dir)
        {
            BlockingMq Mq = BlockingMq.GetMq();
            Config myConfig = Config.GetConfig();
            // check if it matches
            if (SimpleMatch(dir))
            {
                bool scanDir = true;
                bool sendToMq = false;
                // if it does, see what we're gonna do with it
                switch (MatchAction)
                {
                    case MatchAction.Discard:
                        scanDir = false;
                        break;
                    case MatchAction.Snaffle:
                        sendToMq = true;
                        break;
                }

                TreeWalker.DirResult dirResult = new TreeWalker.DirResult()
                {
                    DirPath = dir,
                    Snaffle = sendToMq,
                    ScanDir = scanDir
                };
                return dirResult;
            }
            else return null;
        }
    }
}
