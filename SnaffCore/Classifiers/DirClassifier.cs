using SnaffCore.Concurrency;
using SnaffCore.TreeWalk;
using Config = SnaffCore.Config.Config;

namespace Classifiers
{
    public class DirClassifier
    {
        private ClassifierRule ClassifierRule { get; set; }

        public DirClassifier(ClassifierRule inRule)
        {
            this.ClassifierRule = inRule;
        }

        public DirResult ClassifyDir(string dir)
        {
            BlockingMq Mq = BlockingMq.GetMq();
            Config myConfig = Config.GetConfig();

            DirResult dirResult = new DirResult()
            {
                DirPath = dir,
                Triage = ClassifierRule.Triage,
                ScanDir = true,
            };
            // check if it matches
            TextClassifier textClassifier = new TextClassifier(ClassifierRule);
            if (textClassifier.SimpleMatch(dir))
            {
                // if it does, see what we're gonna do with it
                switch (ClassifierRule.MatchAction)
                {
                    case MatchAction.Discard:
                        dirResult.ScanDir = false;
                        return dirResult;
                    case MatchAction.Snaffle:
                        dirResult.ScanDir = false;
                        Mq.DirResult(dirResult);
                        return dirResult;
                    default:
                        Mq.Error("You've got a misconfigured file ClassifierRule named " + ClassifierRule.RuleName + ".");
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
