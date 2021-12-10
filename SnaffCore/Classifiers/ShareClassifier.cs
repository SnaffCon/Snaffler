using SnaffCore.Concurrency;
using System;
using System.IO;
using static SnaffCore.Config.Options;

namespace Classifiers
{
    public class ShareClassifier
    {
        private ClassifierRule ClassifierRule { get; set; }

        public ShareClassifier(ClassifierRule inRule)
        {
            this.ClassifierRule = inRule;
        }

        public bool ClassifyShare(string share)
        {
            BlockingMq Mq = BlockingMq.GetMq();

            // check if the share has a matching classifier
            TextClassifier textClassifier = new TextClassifier(ClassifierRule);
            TextResult textResult = textClassifier.TextMatch(share);
            if (textResult != null)
            {
                // if it does, see what we're gonna do with it
                switch (ClassifierRule.MatchAction)
                {
                    case MatchAction.Discard:
                        return true;
                    case MatchAction.Snaffle:
                        // in this context snaffle means 'send a report up the queue, and scan the share further'
                        if (IsShareReadable(share))
                        {
                            ShareResult shareResult = new ShareResult()
                            {
                                Triage = ClassifierRule.Triage,
                                Listable = true,
                                SharePath = share
                            };
                            Mq.ShareResult(shareResult);
                        }
                        return false;
                    default:
                        Mq.Error("You've got a misconfigured share ClassifierRule named " + ClassifierRule.RuleName + ".");
                        return false;
                }
            }
            return false;
        }

        internal bool IsShareReadable(string share)
        {
            BlockingMq Mq = BlockingMq.GetMq();
            try
            {
                string[] files = Directory.GetFiles(share);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
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
        public string ShareComment { get; set; }
        public bool Listable { get; set; }
        public bool RootWritable { get; set; }
        public bool RootReadable { get; set; }
        public bool RootModifyable { get; set; }
        public Triage Triage { get; set; } = Triage.Gray; 
    }
}