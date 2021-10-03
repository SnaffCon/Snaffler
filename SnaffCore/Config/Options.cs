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
        public int InterestLevel { get; set; } = 0;

        // Concurrency Options
        public int MaxThreads { get; set; } = 60;
        public int ShareThreads { get; set; }
        public int TreeThreads { get; set; }
        public int FileThreads { get; set; }
        public int MaxFileQueue { get; set; } = 200000;
        public int MaxTreeQueue { get; set; } = 0;
        public int MaxShareQueue { get; set; } = 0;

        // Logging Options
        public bool LogToFile { get; set; } = false;
        public string LogFilePath { get; set; }
        public bool LogTSV { get; set; } = false;
        public char Separator { get; set; } = ' ';
        public bool LogToConsole { get; set; } = true;
        public string LogLevelString { get; set; } = "info";

        // ShareFinder Options
        public bool ShareFinderEnabled { get; set; } = true;
        public string TargetDomain { get; set; }
        public string TargetDc { get; set; }
        public bool LogDeniedShares { get; set; } = false;   //HCK_PR  New config option 

        // FileScanner Options
        public bool DomainUserRules { get; set; } = false;

        // passwords to try on certs that require one
        public List<string> CertPasswords = new List<string>()
        {
            //found in various online tutorials etc
            "",
            "password",
            "mimikatz",
            "1234",
            "abcd",
            "secret",
            "MyPassword",
            "myPassword",
            "MyClearTextPassword",
            "ThePasswordToKeyonPFXFile",
            "P@ssw0rd",
            "testpassword",
            "@OurPassword1",
            "@de08nt2128",
            "changeme",
            "changeit",
            "SolarWinds.R0cks"

        };

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
        public long MaxSizeToGrep { get; set; } = 1000000;

        // these enable or disable automated downloading of files that match the criteria
        public bool Snaffle { get; set; } = false;
        public long MaxSizeToSnaffle { get; set; } = 10000000;
        public string SnafflePath { get; set; }

        // Content processing options
        public int MatchContextBytes { get; set; } = 200;

        public Options()
        {
            //PrepareClassifiers();
            //BuildDefaultClassifiers();
        }
    }
}
