using System.Collections.Generic;

namespace SnaffCore.Definitions
{
    public class Classifier
    {
        // define a way to chain classifiers together
        public string ClassifierName { get; set; }
        public MatchAction MatchAction { get; set; }
        public string RelayTarget { get; set; }
        
        // define the behaviour of this classifier
        public MatchLoc MatchLocation { get; set; }
        public MatchListType WordListType { get; set; }
        public List<string> WordList { get; set; }
        
        // define the severity of this classification
        public Triage Triage { get; set; }
    }
    
    
    public enum MatchLoc
    {
        FilePath,
        FileName,
        FileExtension,
        FileContent
    }

    public enum MatchListType
    {
        Exact,
        Contains,
        Regex
    }

    public enum MatchAction
    {
        Discard,
        Snaffle,
        Relay
    }

    public enum Triage
    {
        Black,
        Green,
        Yellow,
        Red
    }
}