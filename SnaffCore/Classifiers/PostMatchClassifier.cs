using SnaffCore.Concurrency;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static SnaffCore.Config.Options;

namespace SnaffCore.Classifiers
{
    public class PostMatchClassifier
    {
        private ClassifierRule ClassifierRule { get; set; }

        public PostMatchClassifier(ClassifierRule inRule)
        {
            this.ClassifierRule = inRule;
        }

        public bool ClassifyPostMatch(FileInfo fileInfo, AlternativeFileInfo altFileInfo = null)
        {
            BlockingMq Mq = BlockingMq.GetMq();
            // figure out what part we gonna look at
            string stringToMatch = null;

            // Setup file info
            string fileName = null;
            string fullFileName = null;
            string extension = null;
            if (altFileInfo != null)
            {
                fileName = altFileInfo.AlternativeFileName;
                fullFileName = altFileInfo.AlternativeFullFileName;
                extension = altFileInfo.AlternativeExtension;
                Mq.Trace("File " + fileInfo.FullName + " is now handled as " + fullFileName);
            }
            else
            {
                fileName = fileInfo.Name;
                fullFileName = fileInfo.FullName;
                extension = fileInfo.Extension;
            }

            switch (ClassifierRule.MatchLocation)
            {
                case MatchLoc.FileExtension:
                    //stringToMatch = fileInfo.Extension;
                    stringToMatch = extension;
                    // special handling to treat files named like 'thing.kdbx.bak'
                    if (stringToMatch == ".bak")
                    {
                        // strip off .bak
                        //string subName = fileInfo.Name.Replace(".bak", "");
                        string subName = fileName.Substring(0, (fileName.Length - 4)); // Remove ".bak";
                        stringToMatch = Path.GetExtension(subName);
                        // if this results in no file extension, put it back.
                        if (stringToMatch == "")
                        {
                            stringToMatch = ".bak";
                        }
                    }
                    // this is insane that i have to do this but apparently files with no extension return
                    // this bullshit
                    if (stringToMatch == "")
                    {
                        return false;
                    }
                    break;
                case MatchLoc.FileName:
                    //stringToMatch = fileInfo.Name;
                    stringToMatch = fileName;
                    break;
                case MatchLoc.FilePath:
                    //stringToMatch = fileInfo.FullName;
                    stringToMatch = fullFileName;
                    break;
                case MatchLoc.FileLength:
                    if (!SizeMatch(fileInfo))
                    {
                        return false;
                    }
                    else break;
                default:
                    Mq.Error("You've got a misconfigured file classifier rule named " + ClassifierRule.RuleName + ".");
                    return false;
            }

            TextResult textResult = null;

            if (!String.IsNullOrEmpty(stringToMatch))
            {
                TextClassifier textClassifier = new TextClassifier(ClassifierRule);
                // check if it matches
                textResult = textClassifier.TextMatch(stringToMatch);
                if (textResult == null)
                {
                    // if it doesn't we just bail now.
                    return false;
                }
            }

            //FileResult fileResult; // not used
            // if it matches, see what we're gonna do with it
            switch (ClassifierRule.MatchAction)
            {
                case MatchAction.Discard:
                    // chuck it.
                    return true;
                default:
                    Mq.Error("You've got a misconfigured PostMatch rule named " + ClassifierRule.RuleName + ". Only the Discard action is supported for PostMatch rules.");
                    return false;
            }

            //return false; // not used
        }

        public bool SizeMatch(FileInfo fileInfo)
        {
            if (this.ClassifierRule.MatchLength == fileInfo.Length)
            {
                return true;
            }
            return false;
        }
    }


}