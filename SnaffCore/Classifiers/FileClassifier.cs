using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SnaffCore.Concurrency;
using Config = SnaffCore.Config.Config;

namespace Classifiers
{
    public partial class Classifier
    {
        public void ClassifyFile(FileInfo fileInfo)
        {
            BlockingMq Mq = BlockingMq.GetMq();
            Config myConfig = Config.GetConfig();
            // figure out what part we gonna look at
            string stringToMatch = null;

            switch (MatchLocation)
            {
                case MatchLoc.FileExtension:
                    stringToMatch = fileInfo.Extension;
                    break;
                case MatchLoc.FileName:
                    stringToMatch = fileInfo.Name;
                    break;
                case MatchLoc.FilePath:
                    stringToMatch = fileInfo.FullName;
                    break;
                default:
                    Mq.Error("You've got a misconfigured file classifier named " + this.ClassifierName + ".");
                    return;
            }

            if (!String.IsNullOrEmpty(stringToMatch))
            {
                // check if it matches
                if (!SimpleMatch(stringToMatch))
                {
                    // if it doesn't we just bail now.
                    return;
                }
            }

            FileResult fileResult;
            // if it matches, see what we're gonna do with it
            switch (MatchAction)
            {
                case MatchAction.Discard:
                    // chuck it.
                    return;
                case MatchAction.Snaffle:
                    // snaffle that bad boy
                    fileResult = new FileResult()
                    {
                        FileInfo = fileInfo,
                        RwStatus = CanRw(fileInfo),
                        MatchedClassifier = this
                    };
                    Mq.FileResult(fileResult);
                    return;
                case MatchAction.CheckForKeys:
                    // TODO this makes me sad cos it should be in the Content context but this way is much easier.
                    // do a special x509 dance
                    if (x509PrivKeyMatch(fileInfo))
                    {
                        fileResult = new FileResult()
                        {
                            FileInfo = fileInfo,
                            RwStatus = CanRw(fileInfo),
                            MatchedClassifier = this
                        };
                    }
                    return;
                case MatchAction.Relay:
                    // bounce it on to the next classifier
                    // TODO concurrency uplift make this a new task on the poolq
                    Classifier nextClassifier =
                        myConfig.Options.Classifiers.First(thing => thing.ClassifierName == this.RelayTarget);
                    if (nextClassifier.EnumerationScope == EnumerationScope.ContentsEnumeration)
                    {
                        nextClassifier.ClassifyContent(fileInfo);
                    }
                    else if (nextClassifier.EnumerationScope == EnumerationScope.FileEnumeration)
                    {
                        nextClassifier.ClassifyFile(fileInfo);
                    }
                    else
                    {
                        Mq.Error("You've got a misconfigured file classifier named " + this.ClassifierName + ".");
                    }
                    return;
                case MatchAction.EnterArchive:
                // do a special looking inside archive files dance using
                // https://github.com/adamhathcock/sharpcompress
                    // TODO FUUUUUCK
                throw new NotImplementedException("Haven't implemented walking dir structures inside archives. Prob needs pool queue.");
                    return;
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