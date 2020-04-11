using System;
using System.IO;
using SnaffCore.Concurrency;
using Config = SnaffCore.Config.Config;

namespace Classifiers
{
    public class ContentClassifier
    {
        private Classifier classifier { get; set; }

        public ContentClassifier(Classifier inClassifier)
        {
            this.classifier = inClassifier;
        }

        public void ClassifyContent(FileInfo fileInfo)
        {
            BlockingMq Mq = BlockingMq.GetMq();
            Config myConfig = Config.GetConfig();
            FileResult fileResult;
            if (myConfig.Options.MaxSizeToGrep >= fileInfo.Length)
            {
                // figure out if we need to look at the content as bytes or as string.
                switch (classifier.MatchLocation)
                {
                    case MatchLoc.FileContentAsBytes:
                        byte[] fileBytes = File.ReadAllBytes(fileInfo.FullName);
                        if (classifier.ByteMatch(fileBytes))
                        {
                            fileResult = new FileResult()
                            {
                                FileInfo = fileInfo,
                                RwStatus = classifier.CanRw(fileInfo),
                                MatchedClassifier = classifier
                            };
                            Mq.FileResult(fileResult);
                        }

                        return;
                    case MatchLoc.FileContentAsString:
                        string fileString = File.ReadAllText(fileInfo.FullName);
                        if (classifier.SimpleMatch(fileString))
                        {
                            fileResult = new FileResult()
                            {
                                FileInfo = fileInfo,
                                RwStatus = classifier.CanRw(fileInfo),
                                MatchedClassifier = classifier
                            };
                            Mq.FileResult(fileResult);
                        }

                        return;
                    default:
                        Mq.Error("You've got a misconfigured file classifier named " + classifier.ClassifierName + ".");
                        return;
                }
            }
            else
            {
                Mq.Trace("The following file was bigger than the MaxSizeToGrep config parameter:" + fileInfo.FullName);
            }
        }
    }
}