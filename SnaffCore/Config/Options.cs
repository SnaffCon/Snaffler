using System.Collections.Generic;
using System.Security.Principal;
using System.Runtime.Serialization;
using SnaffCore.ActiveDirectory;
using CsToml;

namespace SnaffCore.Config
{
    public enum LogType
    {
        Plain = 0,
        JSON = 1
    }

    [TomlSerializedObject]
    public partial class Options
    {
        public static Options MyOptions { get; set; }

        // Manual Targeting Options
        [TomlValueOnSerialized]
        public List<string> PathTargets { get; set; } = new List<string>();
        [TomlValueOnSerialized]
        public string[] ComputerTargets { get; set; }
        [TomlValueOnSerialized]
        public string ComputerTargetsLdapFilter { get; set; } = "(objectClass=computer)";
        [TomlValueOnSerialized]
        public string ComputerExclusionFile { get; set; }
        [TomlValueOnSerialized]
        public List<string> ComputerExclusions { get; set; } = new List<string>();
        [TomlValueOnSerialized]
        public bool ScanSysvol { get; set; } = true;
        [TomlValueOnSerialized]
        public bool ScanNetlogon { get; set; } = true;
        [TomlValueOnSerialized]
        public bool ScanFoundShares { get; set; } = true;
        [TomlValueOnSerialized]
        public int InterestLevel { get; set; } = 0;
        [TomlValueOnSerialized]
        public bool DfsOnly { get; set; } = false;
        [TomlValueOnSerialized]
        public bool DfsShareDiscovery { get; set; } = false;
        [IgnoreDataMember]
        public Dictionary<string, string> DfsSharesDict { get; set; } = new Dictionary<string, string>();
        [TomlValueOnSerialized]
        public List<string> DfsNamespacePaths { get; set; } = new List<string>();
        [TomlValueOnSerialized]
        public string CurrentUser { get; set; } = WindowsIdentity.GetCurrent().Name;
        [TomlValueOnSerialized]
        public string RuleDir { get; set; }

        [TomlValueOnSerialized]
        public int TimeOut { get; set; } = 5;

        // Concurrency Options
        [TomlValueOnSerialized]
        public int MaxThreads { get; set; } = 60;
        [TomlValueOnSerialized]
        public int ShareThreads { get; set; }
        [TomlValueOnSerialized]
        public int TreeThreads { get; set; }
        [TomlValueOnSerialized]
        public int FileThreads { get; set; }
        [TomlValueOnSerialized]
        public int MaxFileQueue { get; set; } = 200000;
        [TomlValueOnSerialized]
        public int MaxTreeQueue { get; set; } = 0;
        [TomlValueOnSerialized]
        public int MaxShareQueue { get; set; } = 0;

        // Logging Options
        [TomlValueOnSerialized]
        public bool LogToFile { get; set; } = false;
        [TomlValueOnSerialized]
        public string LogFilePath { get; set; }
        [TomlValueOnSerialized]
        public LogType LogType { get; set; }
        [TomlValueOnSerialized]
        public bool LogTSV { get; set; } = false;
        [TomlValueOnSerialized]
        public char Separator { get; set; } = ' ';
        [TomlValueOnSerialized]
        public bool LogToConsole { get; set; } = true;
        [TomlValueOnSerialized]
        public string LogLevelString { get; set; } = "info";

        // ShareFinder Options
        [TomlValueOnSerialized]
        public bool ShareFinderEnabled { get; set; } = true;
        [TomlValueOnSerialized]
        public string TargetDomain { get; set; }
        [TomlValueOnSerialized]
        public string TargetDc { get; set; }
        [TomlValueOnSerialized]
        public bool LogDeniedShares { get; set; } = false; 

        // FileScanner Options
        [TomlValueOnSerialized]
        public bool DomainUserRules { get; set; } = false;
        [TomlValueOnSerialized]
        public int DomainUserMinLen { get; set; } = 6;
        [TomlValueOnSerialized]
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
        [TomlValueOnSerialized]
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

        [TomlValueOnSerialized]
        public List<string> DomainUserStrictStrings { get; set; } 

        [TomlValueOnSerialized]
        public List<string> DomainUsersWordlistRules { get; set; } = new List<string>()
        {
            "KeepConfigRegexRed"
        };

        // this sets the maximum size of file to look inside.
        [TomlValueOnSerialized]
        public long MaxSizeToGrep { get; set; } = 1000000;

        // these enable or disable automated downloading of files that match the criteria
        [TomlValueOnSerialized]
        public bool Snaffle { get; set; } = false;
        [TomlValueOnSerialized]
        public long MaxSizeToSnaffle { get; set; } = 10000000;
        [TomlValueOnSerialized]
        public string SnafflePath { get; set; }

        // Content processing options
        [TomlValueOnSerialized]
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
