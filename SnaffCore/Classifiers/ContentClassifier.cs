using System;
using System.IO;
using System.Text.RegularExpressions;
using SnaffCore.Concurrency;
using Config = SnaffCore.Config.Config;

namespace Classifiers
{
    public class ContentClassifier
    {
        private ClassifierRule ClassifierRule { get; set; }

        public ContentClassifier(ClassifierRule inRule)
        {
            this.ClassifierRule = inRule;
        }

        public void ClassifyContent(FileInfo fileInfo)
        {
            BlockingMq Mq = BlockingMq.GetMq();
            Config myConfig = Config.GetConfig();
            FileResult fileResult;
            if (myConfig.Options.MaxSizeToGrep >= fileInfo.Length)
            {
                // figure out if we need to look at the content as bytes or as string.
                switch (ClassifierRule.MatchLocation)
                {
                    case MatchLoc.FileContentAsBytes:
                        byte[] fileBytes = File.ReadAllBytes(fileInfo.FullName);
                        if (ByteMatch(fileBytes))
                        {
                            fileResult = new FileResult(fileInfo)
                            {
                                MatchedRule = ClassifierRule
                            };
                            Mq.FileResult(fileResult);
                        }
                        return;
                    case MatchLoc.FileContentAsString:
                        string fileString = File.ReadAllText(fileInfo.FullName);
                        TextClassifier textClassifier = new TextClassifier(ClassifierRule);
                        TextResult textResult = textClassifier.SimpleMatch(fileString);
                        if (textResult != null)
                        {
                            fileResult = new FileResult(fileInfo)
                            {
                                MatchedRule = ClassifierRule
                            };
                            Mq.FileResult(fileResult);
                        }
                        return;
                    default:
                        Mq.Error("You've got a misconfigured file ClassifierRule named " + ClassifierRule.RuleName + ".");
                        return;
                }
            }
            else
            {
                Mq.Trace("The following file was bigger than the MaxSizeToGrep config parameter:" + fileInfo.FullName);
            }
        }

        public bool ByteMatch(byte[] fileBytes)
        {
            // TODO
            throw new NotImplementedException(message: "Haven't implemented byte-based content searching yet lol.");
        }
    }
}