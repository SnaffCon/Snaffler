
using System.Collections.Generic;

namespace Classifiers
{
    public class ClassifierRule
    {
        // define in what phase this rule is applied
        public EnumerationScope EnumerationScope { get; set; } = EnumerationScope.FileEnumeration;

        // define a way to chain rules together
        public string RuleName { get; set; } = "Default";
        public MatchAction MatchAction { get; set; } = MatchAction.Snaffle;
        public string RelayTarget { get; set; } = null;
        public int RuleOrder { get; set; } = 0;

        // define the behaviour of this rule
        public MatchLoc MatchLocation { get; set; } = MatchLoc.FileName;
        public MatchListType WordListType { get; set; } = MatchListType.Contains;
        public List<string> WordList { get; set; } = new List<string>();

        // define the severity of any matches
        public Triage Triage { get; set; } = Triage.Black;
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
        FileContentAsBytes
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