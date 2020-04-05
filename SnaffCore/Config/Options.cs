using System;
using System.Collections.Generic;
using NLog;
using SnaffCore.Definitions;

namespace SnaffCore.Config
{
    public partial class Options
    {
        public bool LogToFile { get; set; } = false;
        public string LogFilePath { get; set; }
        public bool LogToConsole { get; set; } = true;
        public LogLevel LogLevel { get; set; } = LogLevel.Info;
        public List<Classifier> Classifiers { get; set; } = new List<Classifier>();
        
        // lists which correspond to classifier settings
        [Nett.TomlIgnore] public List<string> DiscardShareExact { get; set; }
        [Nett.TomlIgnore] public List<string> KeepExtExact { get; set; }
        [Nett.TomlIgnore] public List<string> DiscardExtExact { get; set; }
        [Nett.TomlIgnore] public List<string> KeepFilenameExact { get; set; } 
        [Nett.TomlIgnore] public List<string> DiscardFilepathContains { get; set; } 
        [Nett.TomlIgnore] public List<string> GrepExtExact { get; set; }
        [Nett.TomlIgnore] public List<string> KeepFilepathContains { get; set; }
        
        // classifier lists still needing to be classifier lists
        [Nett.TomlIgnore]
        public List<string> NameStringsToKeep { get; set; } =
        new List<string>(){
            // these are strings that make a file NAME interesting if found within.
            "passw",
            "as-built",
            "handover",
            "secret",
            "thycotic",
            "cyberark",
            "_rsa",
            "_dsa",
            "_ed25519",
            "_ecdsa"
        };
        
        // other lists
        [Nett.TomlIgnore]
        public string[] ShareStringsToPrioritise { get; set; } =
        {
            // these are substrings that make a share or hostname more interesting and make it worth prioritising.
            "IT",
            "security",
            "admin",
            "dev",
            "sql",
            "backup",
            "sap",
            "erp",
            "oracle",
            "vmware",
            "sccm"
        };


        public Options(bool defaults = false)
        {
            if (defaults)
            {
                BuildDefaultClassifiers();
            }
        }

        public void PrepareClassifiers()
        {
            if (this.Classifiers.Count <= 0)
            {
                this.BuildDefaultClassifiers();
            }
        }

        private void BuildDefaultClassifiers()
        {
            this.Classifiers = new List<Classifier>
            {
                new Classifier()
                { // corresponds to DiscardShareExact
                    ClassifierName = "DiscardShareExact",
                    EnumerationScope = EnumerationScope.ShareEnumeration,
                    MatchLocation = MatchLoc.ShareName,
                    MatchAction = MatchAction.Discard,
                    WordListType = MatchListType.EndsWith,
                    WordList = new List<string>()
                    {
                        // these are share names that make us skip the share instantly.
                        "print$",
                        "ipc",
                        ""
                    },
                },
                new Classifier()
                {
                    ClassifierName = "DiscardFilepathContains",
                    EnumerationScope = EnumerationScope.DirectoryEnumeration,
                    MatchLocation = MatchLoc.FilePath,
                    MatchAction = MatchAction.Discard,
                    WordListType = MatchListType.Contains,
                    WordList = new List<string>()
                    {
                        // these are directory names that make us skip a dir instantly when building a tree.
                        "winsxs",
                        "syswow64",
                        "system32",
                        "systemapps",
                        "servicing\\packages",
                        "Microsoft.NET\\Framework",
                        "ADMIN$\\immersivecontrolpanel",
                        "windows\\immersivecontrolpanel",
                        "ADMIN$\\diagnostics",
                        "windows\\diagnostics",
                        "ADMIN$\\debug", // might want to put this one back in it's kind of interesting?
                        "windows\\debug",
                        "node_modules",
                        "vendor\\bundle",
                        "vendor\\cache",
                        "locale\\",
                        "localization\\",
                        "\\AppData\\Local\\Microsoft\\",
                        "\\AppData\\Roaming\\Microsoft\\",
                        "\\wsuscontent"
                    },
                },
                new Classifier()
                {
                    ClassifierName = "KeepExtExact",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileExtension,
                    MatchAction = MatchAction.Snaffle,
                    WordListType = MatchListType.Exact,
                    WordList = new List<string>()
                    {
                        // these are file extensions that we will always want to keep no matter what.
                        ".kdbx",
                        ".kdb",
                        ".ppk",
                        ".vnc",
                        ".vmdk",
                        ".vhd",
                        ".rdp",
                        ".der",
                        ".key",
                        ".pk12",
                        ".bak",
                        ".p12",
                        ".pkcs12",
                        ".pfx",
                        ".asc",
                        ".ovpn",
                        ".cscfg",
                        ".mdf",
                        ".sdf",
                        ".sqlite",
                        ".sqlite3",
                        ".bek",
                        ".tpm",
                        ".fve",
                        ".psafe3",
                        ".jks",
                        ".pcap",
                        ".kwallet",
                        ".tblk",
                        ".key",
                        ".keypair",
                        ".sqldump",
                        ".ova",
                        ".ovf",
                        ".wim"
                    },
                    
                },
                new Classifier()
                {
                    ClassifierName = "DiscardExtExact",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileExtension,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.Discard,
                    WordList = new List<string>()
                    {
                        // always skip these file extensions
                        ".jpg",
                        ".jpeg",
                        ".jpe",
                        ".jif",
                        ".jfif",
                        ".jfi",
                        ".webp",
                        ".ico",
                        ".psd",
                        ".png",
                        ".gif",
                        ".bmp",
                        ".tiff",
                        ".tif",
                        ".otf",
                        ".eps",
                        ".xcf",
                        ".ttf",
                        ".lock",
                        ".css",
                        ".less"
                    },
                },
                new Classifier()
                {
                    ClassifierName = "KeepFilenameExact",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileName,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.Snaffle,
                    WordList = new List<string>()
                    {
                        // these are file names that we always want to keep if it's an exact match.
                        "id_rsa",
                        "id_dsa",
                        "NTDS.DIT",
                        "shadow",
                        "pwd.db",
                        "unattend.xml",
                        ".netrc",
                        "_netrc",
                        ".htaccess",
                        "otr.private_key",
                        ".secret_token.rb",
                        "carrierwave.rb",
                        "database.yml",
                        "omniauth.rb",
                        "settings.py",
                        ".agilekeychain",
                        ".keychain",
                        "jenkins.plugins.publish_over_ssh.BapSshPublisherPlugin.xml",
                        "credentials.xml",
                        "LocalSettings.php",
                        "Favorites.plist",
                        "knife.rb",
                        "proxy.config",
                        "proftpdpasswd",
                        "robomongo.json",
                        "filezilla.xml",
                        "recentservers.xml",
                        "terraform.tfvars",
                        ".exports",
                        ".functions",
                        ".extra",
                        "bash_history",
                        "zsh_history",
                        "sh_history",
                        "zhistory",
                        "mysql_history",
                        "psql_history",
                        ".pgpass",
                        ".irb_history",
                        ".dbeaver-data-sources.xml",
                        ".s3vfg",
                        "sftp-config.json",
                        "config.inc",
                        "config.php",
                        "keystore",
                        "keyring",
                        ".tugboat",
                        ".git-credentials",
                        ".gitconfig",
                        "passwd",
                        "shadow",
                        ".dockercfg",
                        ".npmrc",
                        ".env"
                    },
                },
                new Classifier()
                {
                    ClassifierName = "GrepExtExact",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileExtension,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.Grep,
                    WordList = new List<string>()
                    {
                        // these are file extensions that tell us the file is worth grepping.
                        ".ps1",
                        ".bat",
                        ".wsf",
                        ".vbs",
                        ".pl",
                        ".txt",
                        ".cs",
                        ".ascx",
                        ".java",
                        ".config",
                        ".ini",
                        ".inf",
                        ".cnf",
                        ".conf",
                        ".py",
                        ".php",
                        ".aspx",
                        ".ashx",
                        ".asmx",
                        ".asp",
                        ".jsp",
                        ".yaml",
                        ".xml",
                        ".json",
                        ".psd1",
                        ".psm1",
                        ".sh",
                        ".cshtml",
                        ".sql",
                        ".pem",
                        ".log"
                    },
                },
                new Classifier()
                {
                    ClassifierName = "KeepFilepathContains",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FilePath,
                    WordListType = MatchListType.Contains,
                    MatchAction = MatchAction.Snaffle,
                    WordList = new List<string>()
                    {
                        ".ssh\\config",
                        ".purple\\accounts.xml",
                        ".aws\\credentials",
                        ".gem\\credentials",
                        "doctl\\config.yaml",
                        "config\\hub"
                    },
                },
            };
        }
    }
}