using System.Collections.Generic;
using System.Security.Principal;
using SnaffCore.ActiveDirectory;

namespace SnaffCore.Config
{
    public enum LogType
    {
        Plain = 0,
        JSON = 1
    }

    public partial class Options
    {
        public static Options MyOptions { get; set; }

        // Pause and resume functionality
        public string TaskFile { get; set; }
        public double TaskFileTimeOut { get; set; } = 5;
        public string ResumeFrom { get; set; }

        // Manual Targeting Options
        public List<string> PathTargets { get; set; } = new List<string>();
        public string[] ComputerTargets { get; set; }
        public string ComputerTargetsLdapFilter { get; set; } = "(objectClass=computer)";
        public string ComputerExclusionFile { get; set; }
        public List<string> ComputerExclusions { get; set; } = new List<string>();
        public bool ScanSysvol { get; set; } = true;
        public bool ScanNetlogon { get; set; } = true;
        public bool ScanFoundShares { get; set; } = true;
        public int InterestLevel { get; set; } = 0;
        public bool DfsOnly { get; set; } = false;
        public bool DfsShareDiscovery { get; set; } = false;
        [Nett.TomlIgnore]
        public Dictionary<string, string> DfsSharesDict { get; set; } = new Dictionary<string, string>();
        public List<string> DfsNamespacePaths { get; set; } = new List<string>();
        public string CurrentUser { get; set; } = WindowsIdentity.GetCurrent().Name;
        public string RuleDir { get; set; }

        public int TimeOut { get; set; } = 5;

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
        public LogType LogType { get; set; }
        public bool LogTSV { get; set; } = false;
        public char Separator { get; set; } = ' ';
        public bool LogToConsole { get; set; } = true;
        public string LogLevelString { get; set; } = "info";

        // ShareFinder Options
        public bool ShareFinderEnabled { get; set; } = true;
        public string TargetDomain { get; set; }
        public string TargetDc { get; set; }
        public bool LogDeniedShares { get; set; } = false; 

        // FileScanner Options
        public bool DomainUserRules { get; set; } = false;
        public int DomainUserMinLen { get; set; } = 6;
        public DomainUserNamesFormat[] DomainUserNameFormats { get; set; } = new DomainUserNamesFormat[] { DomainUserNamesFormat.sAMAccountName };

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

        // initialize a list for this.  We will build it dynamically so don't allow for toml setting
        public List<string> DomainUsersToMatch = new List<string>();

        // These options can be set in toml. They need the get/set accessor
        public List<string> DomainUserMatchStrings { get; set; } = new List<string>()
        {
            "sql",
            "svc",
            "service",
            "backup",
            "ccm",
            "scom",
            "opsmgr",
            "adm",
            "adcs",
            "MSOL",
            "adsync",
            "thycotic",
            "secretserver",
            "cyberark",
            "configmgr"
        };

        public List<string> DomainUserStrictStrings { get; set; } 

        public List<string> DomainUsersWordlistRules { get; set; } = new List<string>()
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

        public enum DomainUserNamesFormat
        {
            sAMAccountName,
            NetBIOS,
            UPN
        }
    }
}
