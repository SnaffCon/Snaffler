using SnaffCore.Concurrency;
using SnaffCore.TreeWalk;
using Config = SnaffCore.Config.Config;

namespace Classifiers
{
    public class DirClassifier
    {
        private Classifier classifier { get; set; }

        public DirClassifier(Classifier inClassifier)
        {
            this.classifier = inClassifier;
        }

        public DirResult ClassifyDir(string dir)
        {
            BlockingMq Mq = BlockingMq.GetMq();
            Config myConfig = Config.GetConfig();

            DirResult dirResult = new DirResult()
            {
                DirPath = dir,
                Triage = classifier.Triage,
                ScanDir = true,
            };
            // check if it matches
            TextClassifier textClassifier = new TextClassifier(classifier);
            if (textClassifier.SimpleMatch(dir))
            {
                // if it does, see what we're gonna do with it
                switch (classifier.MatchAction)
                {
                    case MatchAction.Discard:
                        dirResult.ScanDir = false;
                        return dirResult;
                    case MatchAction.Snaffle:
                        dirResult.ScanDir = false;
                        Mq.DirResult(dirResult);
                        return dirResult;
                    default:
                        Mq.Error("You've got a misconfigured file classifier named " + classifier.ClassifierName + ".");
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
