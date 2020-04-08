using SnaffCore.Concurrency;
using SnaffCore.ShareScan;
using Config = SnaffCore.Config.Config;

namespace Classifiers
{
    public partial class Classifier
    {
        public DirResult ClassifyDir(string dir)
        {
            BlockingMq Mq = BlockingMq.GetMq();
            Config myConfig = Config.GetConfig();
            // check if it matches
            if (SimpleMatch(dir))
            {
                bool scanDir = true;
                // if it does, see what we're gonna do with it
                DirResult dirResult = new DirResult()
                {
                    DirPath = dir,
                    Triage = Triage,
                    ScanDir = scanDir,
                };
                switch (MatchAction)
                {
                    case MatchAction.Discard:
                        scanDir = false;
                        break;
                    case MatchAction.Snaffle:
                        Mq.DirResult(dirResult);
                        break;
                    default:
                        Mq.Error("You've got a misconfigured file classifier named " + this.ClassifierName + ".");
                        break;
                }
                return dirResult;
            }
            else return null;
        }
    }

    public class DirResult
    {
        public bool ScanDir { get; set; }
        public string DirPath { get; set; }
        public Triage Triage { get; set; }
    }
}
