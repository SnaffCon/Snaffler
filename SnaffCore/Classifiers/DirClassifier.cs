using System.IO;
using SnaffCore.Classifiers.EffectiveAccess;
using SnaffCore.Concurrency;
using static SnaffCore.Config.Options;

namespace SnaffCore.Classifiers
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
            DirResult dirResult = new DirResult()
            {
                DirPath = dir,
                Triage = ClassifierRule.Triage,
                ScanDir = true,
            };

            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            EffectivePermissions effPerms = new EffectivePermissions(MyOptions.CurrentUser);
            dirResult.RwStatus = effPerms.CanRw(dirInfo);

            // check if it matches
            TextClassifier textClassifier = new TextClassifier(ClassifierRule);
            TextResult textResult = textClassifier.TextMatch(dir);
            if (textResult != null)
            {
                // if it does, see what we're gonna do with it
                switch (ClassifierRule.MatchAction)
                {
                    case MatchAction.Discard:
                        dirResult.ScanDir = false;
                        if (MyOptions.LogEverything)
                        {
                            Mq.DirResult(dirResult);
                        }
                        return dirResult;
                    case MatchAction.Snaffle:
                        dirResult.Triage = ClassifierRule.Triage;
                        Mq.DirResult(dirResult);
                        return dirResult;
                    default:
                        Mq.Error("You've got a misconfigured file ClassifierRule named " + ClassifierRule.RuleName + ".");
                        return null;
                }
            }

            if (MyOptions.LogEverything)
            {
                Mq.DirResult(dirResult);
            }

            return dirResult;
        }
    }

    public class DirResult
    {
        public bool ScanDir { get; set; }
        public string DirPath { get; set; }
        public RwStatus RwStatus { get; set; }
        public Triage Triage { get; set; }
    }
}
