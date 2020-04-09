using System;
using System.IO;
using SnaffCore.Concurrency;
using Config = SnaffCore.Config.Config;

namespace Classifiers
{
    public partial class Classifier
    {
        public void ClassifyContent(FileInfo fileInfo)
        {
            BlockingMq Mq = BlockingMq.GetMq();
            Config myConfig = Config.GetConfig();
            FileResult fileResult;
            if (myConfig.Options.MaxSizeToGrep >= fileInfo.Length)
            {
                // figure out if we need to look at the content as bytes or as string.
                switch (MatchLocation)
                {
                    case MatchLoc.FileContentAsBytes:
                        byte[] fileBytes = File.ReadAllBytes(fileInfo.FullName);
                        if (ByteMatch(fileBytes))
                        {
                            fileResult = new FileResult()
                            {
                                FileInfo = fileInfo,
                                RwStatus = CanRw(fileInfo),
                                MatchedClassifier = this
                            };
                            Mq.FileResult(fileResult);
                        }

                        return;
                    case MatchLoc.FileContentAsString:
                        string fileString = File.ReadAllText(fileInfo.FullName);
                        if (SimpleMatch(fileString))
                        {
                            fileResult = new FileResult()
                            {
                                FileInfo = fileInfo,
                                RwStatus = CanRw(fileInfo),
                                MatchedClassifier = this
                            };
                            Mq.FileResult(fileResult);
                        }

                        return;
                    default:
                        Mq.Error("You've got a misconfigured file classifier named " + this.ClassifierName + ".");
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