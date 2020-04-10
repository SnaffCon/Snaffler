using SnaffCore.Concurrency;
using SnaffCore.TreeWalk;
using Config = SnaffCore.Config.Config;

namespace Classifiers
{
    public partial class Classifier
    {
        public DirResult ClassifyDir(string dir)
        {
            BlockingMq Mq = BlockingMq.GetMq();
            Config myConfig = Config.GetConfig();

            DirResult dirResult = new DirResult()
            {
                DirPath = dir,
                Triage = Triage,
                ScanDir = true,
            };
            // check if it matches
            if (SimpleMatch(dir))
            {
                // if it does, see what we're gonna do with it
                switch (MatchAction)
                {
                    case MatchAction.Discard:
                        dirResult.ScanDir = false;
                        return dirResult;
                    case MatchAction.Snaffle:
                        dirResult.ScanDir = false;
                        Mq.DirResult(dirResult);
                        return dirResult;
                    default:
                        Mq.Error("You've got a misconfigured file classifier named " + this.ClassifierName + ".");
                        return null;
                }
            }
            return dirResult;
        }
    }

    public class DirResult
    {
        public bool ScanDir { get; set; }
        public string DirPath { get; set; }
        public Triage Triage { get; set; }
    }
}
