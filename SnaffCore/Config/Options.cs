using System.Collections.Generic;

namespace SnaffCore.Config
{
    public partial class Options
    {
        public static Options MyOptions { get; set; }

        // Manual Targeting Options
        public string[] PathTargets { get; set; }
        public string[] ComputerTargets { get; set; }
        public bool ScanSysvol { get; set; } = true;
        public bool ScanNetlogon { get; set; } = true;

        // Concurrency Options
        public int MaxThreads { get; set; } = 30;
        public int ShareThreads { get; set; }
        public int TreeThreads { get; set; }
        public int FileThreads { get; set; }
        public int MaxFileQueue { get; set; } = 200000;
        public int MaxTreeQueue { get; set; } = 0;
        public int MaxShareQueue { get; set; } = 0;

        // Logging Options
        public bool LogToFile { get; set; } = false;
        public string LogFilePath { get; set; }
        public bool LogToConsole { get; set; } = true;
        public string LogLevelString { get; set; } = "info";

        // ShareFinder Options
        public bool ShareFinderEnabled { get; set; } = true;
        public string TargetDomain { get; set; }
        public string TargetDc { get; set; }

        // FileScanner Options
        public bool DomainUserRules { get; set; } = false;
        public List<string> DomainUserMatchStrings = new List<string>()
        {
            "sql",
            "svc",
            "service",
            "backup",
            "ccm",
            "scom",
            "opsmgr",
            "adm"
        };

        public List<string> DomainUsersToMatch = new List<string>();
        public List<string> DomainUsersWordlistRules = new List<string>()
        {
            "KeepConfigRegexRed"
        };

        // this sets the maximum size of file to look inside.
        public long MaxSizeToGrep { get; set; } = 10000000;

        // these enable or disable automated downloading of files that match the criteria
        public bool Snaffle { get; set; } = false;
        public long MaxSizeToSnaffle { get; set; } = 10000000;
        public string SnafflePath { get; set; }

        // Content processing options
        public int MatchContextBytes { get; set; } = 200;

        public Options()
        {
            ShareThreads = MaxThreads / 2;
            TreeThreads = MaxThreads / 4;
            FileThreads = MaxThreads / 4;
            //PrepareClassifiers();
            //BuildDefaultClassifiers();
        }
    }
}