using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using Nett.Coma;
using SnaffCore.Concurrency;
using SnaffCore.Config;
using SnaffCore.ShareScan;
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
                    break;
            }

            // if we're using string-based 
            if (!String.IsNullOrEmpty(stringToMatch))
            {
                // check if it matches
                if (!SimpleMatch(stringToMatch))
                {
                    // if it doesn't we just bail now.
                    return;
                }
            }

            // if it matches, see what we're gonna do with it
            switch (MatchAction)
            {
                case MatchAction.Discard:
                    // chuck it.
                    return;
                case MatchAction.Snaffle:
                    // snaffle that bad boy
                    FileResult fileResult = new FileResult()
                    {
                        FileInfo = fileInfo,
                        RwStatus = CanRw(fileInfo),
                        MatchedClassifier = this
                    };
                    Mq.FileResult(fileResult);
                    break;
                case MatchAction.Grep:
                    // do a special looking for strings in the file dance
                    // TODO FUUUUUCK
                    break;
                case MatchAction.CheckForKeys:
                    // do a special x509 dance
                    // TODO FUUUUUCK
                    break;
                case MatchAction.Relay:
                    // bounce it on to the next classifier
                    Classifier nextClassifier =
                        myConfig.Options.Classifiers.First(thing => thing.ClassifierName == this.RelayTarget);
                    nextClassifier.ClassifyFile(fileInfo);
                    break;
                case MatchAction.EnterArchive:
                    // do a special looking inside archive files dance
                    // TODO FUUUUUCK
                    break;
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