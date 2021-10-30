
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Classifiers
{
    public class ClassifierRule
    {
        // define in what phase this rule is applied
        public EnumerationScope EnumerationScope { get; set; } = EnumerationScope.FileEnumeration;

        // define a way to chain rules together
        public string RuleName { get; set; } = "Default";
        public MatchAction MatchAction { get; set; } = MatchAction.Snaffle;
        public List<string> RelayTargets { get; set; } = null;

        public string Description { get; set; } = "A description of what a rule does.";

        // define the behaviour of this rule
        public MatchLoc MatchLocation { get; set; } = MatchLoc.FileName;
        public MatchListType WordListType { get; set; } = MatchListType.Contains;
        public int MatchLength { get; set; } = 0;
        public string MatchMD5 { get; set; }
        public List<string> WordList { get; set; } = new List<string>();
        public List<Regex> Regexes { get; set; }

        // define the severity of any matches
        public Triage Triage { get; set; } = Triage.Green;
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
        FileContentAsString,
        FileContentAsBytes,
        FileLength,
        FileMD5
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