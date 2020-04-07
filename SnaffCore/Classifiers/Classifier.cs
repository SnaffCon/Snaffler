using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using SnaffCore.Config;

namespace Classifiers
{
    public partial class Classifier
    {
        // define in what phase this classifier is run
        public EnumerationScope EnumerationScope { get; set; } = EnumerationScope.FileEnumeration;

        // define a way to chain classifiers together
        public string ClassifierName { get; set; } = "Default";
        public MatchAction MatchAction { get; set; } = MatchAction.Discard;
        public string RelayTarget { get; set; } = null;

        // define the behaviour of this classifier
        public MatchLoc MatchLocation { get; set; } = MatchLoc.FileName;
        public MatchListType WordListType { get; set; } = MatchListType.Contains;
        public List<string> WordList { get; set; } = new List<string>();

        // define the severity of this classification
        public Triage Triage { get; set; } = Triage.Black;


        // Methods for classification
        internal bool SimpleMatch(string input)
        {
            // generic match checking
            bool match = false;
            switch (this.WordListType)
            {
                case MatchListType.Contains:
                    foreach (string matchString in this.WordList)
                    {
                        if (input.Contains(matchString))
                        {
                            return true;
                        }
                    }

                    break;
                case MatchListType.EndsWith:
                    foreach (string matchString in this.WordList)
                    {
                        if (input.EndsWith(matchString))
                        {
                            return true;
                        }
                    }

                    break;
                case MatchListType.Exact:
                    foreach (string matchString in this.WordList)
                    {
                        if (input == matchString)
                        {
                            return true;
                        }
                    }

                    break;
                case MatchListType.StartsWith:
                    foreach (string matchString in this.WordList)
                    {
                        if (input.StartsWith(matchString))
                        {
                            return true;
                        }
                    }

                    break;
                case MatchListType.Regex:
                    foreach (string matchString in this.WordList)
                    {
                        Regex regex = new Regex(matchString);
                        if (regex.IsMatch(input))
                        {
                            return true;
                        }
                    }
                    break;
                default:
                    return false;
            }
            return false;
        }
    }

    public enum EnumerationScope
    {
        ShareEnumeration,
        DirectoryEnumeration,
        FileEnumeration,
        ContentsEnumeration
    }
    
    public enum MatchLoc
    {
        ShareName,
        FilePath,
        FileName,
        FileExtension,
        FileContent
    }

    public enum MatchListType
    {
        Exact,
        Contains,
        Regex,
        EndsWith,
        StartsWith
    }

    public enum MatchAction
    {
        Discard,
        SendToNextScope,
        Snaffle,
        Grep,
        Relay,
        CheckForKeys,
        EnterArchive
    }

    public enum Triage
    {
        Black,
        Green,
        Yellow,
        Red
    }

}