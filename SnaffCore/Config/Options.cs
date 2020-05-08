using System.Collections.Generic;
using System.Linq;
using Classifiers;

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
        //public int MaxThreads { get; set; } = 30;
        public int ShareThreads { get; set; } = 30;
        public int TreeThreads { get; set; } = 20;
        public int FileThreads { get; set; } = 50;
        public int MaxFileQueue { get; set; } = 200000;
        public int MaxTreeQueue { get; set; } = 20000;
        public int MaxShareQueue { get; set; } = 20000;

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
        public bool CreateDomainUsersRule { get; set; } = false;
        public bool QueryDomainForUsers { get; set; } = true;
        public List<string> DomainUserMatchStrings = new List<string>()
        {
            "sql",
            "svc",
            "service",
            "backup",
            "svr",
            "test",
            "dev",
            "ccm",
            "scom",
            "opsmgr",
            "adm"
        };

        public List<string> DomainUsersToMatch = new List<string>();

        // this sets the maximum size of file to look inside.
        public long MaxSizeToGrep { get; set; } = 1000000;

        // these enable or disable automated downloading of files that match the criteria
        public bool Snaffle { get; set; } = false;
        public long MaxSizeToSnaffle { get; set; } = 10000000;
        public string SnafflePath { get; set; }

        // Content processing options
        public int MatchContextBytes { get; set; } = 50;

        public Options()
        {
            //PrepareClassifiers();
                //BuildDefaultClassifiers();
        }
    }
}