using System.Collections.Generic;

namespace SnaffCore.Definitions
{
    public class Classifier
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
        Snaffle, // basically just download it
        Grep,
        Relay, // send to another classifier
        ////////////////////////
        // specialist actions:
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