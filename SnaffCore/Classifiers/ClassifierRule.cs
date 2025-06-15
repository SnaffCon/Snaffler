
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CsToml;

namespace SnaffCore.Classifiers
{
    [TomlSerializedObject]
    public partial class ClassifierRule
    {
        // define in what phase this rule is applied
        [TomlValueOnSerialized]
        public EnumerationScope EnumerationScope { get; set; } = EnumerationScope.FileEnumeration;

        // define a way to chain rules together
        [TomlValueOnSerialized]
        public string RuleName { get; set; } = "Default";
        [TomlValueOnSerialized]
        public MatchAction MatchAction { get; set; } = MatchAction.Snaffle;
        [TomlValueOnSerialized]
        public List<string> RelayTargets { get; set; } = null;

        [TomlValueOnSerialized]
        public string Description { get; set; } = "A description of what a rule does.";

        // define the behaviour of this rule
        [TomlValueOnSerialized]
        public MatchLoc MatchLocation { get; set; } = MatchLoc.FileName;
        [TomlValueOnSerialized]
        public MatchListType WordListType { get; set; } = MatchListType.Contains;
        [TomlValueOnSerialized]
        public int MatchLength { get; set; } = 0;
        [TomlValueOnSerialized]
        public string MatchMD5 { get; set; }
        [TomlValueOnSerialized]
        public List<string> WordList { get; set; } = new List<string>();
        [TomlValueOnSerialized]
        public List<Regex> Regexes { get; set; }

        // define the severity of any matches
        [TomlValueOnSerialized]
        public Triage Triage { get; set; } = Triage.Green;
    }

    public enum EnumerationScope
    {
        ShareEnumeration,
        DirectoryEnumeration,
        FileEnumeration,
        ContentsEnumeration,
        PostMatch
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
        Red,
        Gray
    }
}