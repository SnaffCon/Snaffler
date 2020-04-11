using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using SnaffCore.Concurrency;
using Config = SnaffCore.Config.Config;

namespace Classifiers
{
    public class FileClassifier
    {
        private Classifier classifier { get; set; }

        public FileClassifier(Classifier inClassifier)
        {
            this.classifier = inClassifier;
        }

        public bool ClassifyFile(FileInfo fileInfo)
        {
            BlockingMq Mq = BlockingMq.GetMq();
            Config myConfig = Config.GetConfig();
            // figure out what part we gonna look at
            string stringToMatch = null;

            switch (classifier.MatchLocation)
            {
                case MatchLoc.FileExtension:
                    stringToMatch = fileInfo.Extension;
                    // this is insane that i have to do this but apparently files with no extension return
                    // this bullshit
                    if (stringToMatch == "")
                    {
                        return true;
                    }
                    break;
                case MatchLoc.FileName:
                    stringToMatch = fileInfo.Name;
                    break;
                case MatchLoc.FilePath:
                    stringToMatch = fileInfo.FullName;
                    break;
                default:
                    Mq.Error("You've got a misconfigured file classifier named " + classifier.ClassifierName + ".");
                    return true;
            }

            if (!String.IsNullOrEmpty(stringToMatch))
            {
                // check if it matches
                if (!classifier.SimpleMatch(stringToMatch))
                {
                    // if it doesn't we just bail now.
                    return false;
                }
            }

            FileResult fileResult;
            // if it matches, see what we're gonna do with it
            switch (classifier.MatchAction)
            {
                case MatchAction.Discard:
                    // chuck it.
                    return true;
                case MatchAction.Snaffle:
                    // snaffle that bad boy
                    fileResult = new FileResult()
                    {
                        FileInfo = fileInfo,
                        RwStatus = classifier.CanRw(fileInfo),
                        MatchedClassifier = classifier
                    };
                    Mq.FileResult(fileResult);
                    return true;
                case MatchAction.CheckForKeys:
                    // TODO this makes me sad cos it should be in the Content context but this way is much easier.
                    // do a special x509 dance
                    if (classifier.x509PrivKeyMatch(fileInfo))
                    {
                        fileResult = new FileResult()
                        {
                            FileInfo = fileInfo,
                            RwStatus = classifier.CanRw(fileInfo),
                            MatchedClassifier = classifier
                        };
                        Mq.FileResult(fileResult);
                    }
                    return true;
                case MatchAction.Relay:
                    // bounce it on to the next classifier
                    // TODO concurrency uplift make this a new task on the poolq
                    try
                    {
                        Classifier nextClassifier =
                            myConfig.Options.Classifiers.First(thing => thing.ClassifierName == classifier.RelayTarget);

                        if (nextClassifier.EnumerationScope == EnumerationScope.ContentsEnumeration)
                        {
                            ContentClassifier contentClassifier = new ContentClassifier(nextClassifier);
                        }
                        else if (nextClassifier.EnumerationScope == EnumerationScope.FileEnumeration)
                        {
                            ClassifyFile(fileInfo);
                        }
                        else
                        {
                            Mq.Error("You've got a misconfigured file classifier named " + classifier.ClassifierName + ".");
                        }
                    }
                    catch (Exception e)
                    {
                        Mq.Trace(e.ToString());
                    }

                    return true;
                case MatchAction.EnterArchive:
                // do a special looking inside archive files dance using
                // https://github.com/adamhathcock/sharpcompress
                    // TODO FUUUUUCK
                throw new NotImplementedException("Haven't implemented walking dir structures inside archives. Prob needs pool queue.");
                    return false;
                default:
                    Mq.Error("You've got a misconfigured file classifier named " + classifier.ClassifierName + ".");
                    return false;
            }
        }
    }

    public class FileResult
    {
        public FileInfo FileInfo { get; set; }
        public GrepFileResult GrepFileResult { get; set; }
        public RwStatus RwStatus { get; set; }
        public Classifier MatchedClassifier { get; set; }
    }

    public class GrepFileResult
    {
        public List<string> GreppedStrings { get; set; }
        public string GrepContext { get; set; }
    }
}