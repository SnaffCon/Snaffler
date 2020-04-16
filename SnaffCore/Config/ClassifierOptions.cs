using System.Collections.Generic;
using Classifiers;

namespace SnaffCore.Config
{
    public partial class Options
    {
        // Classifiers
        public List<ClassifierRule> Classifiers { get; set; } = new List<ClassifierRule>();
        [Nett.TomlIgnore]
        public List<ClassifierRule> ShareClassifiers { get; set; } = new List<ClassifierRule>();
        [Nett.TomlIgnore]
        public List<ClassifierRule> DirClassifiers { get; set; } = new List<ClassifierRule>();
        [Nett.TomlIgnore]
        public List<ClassifierRule> FileClassifiers { get; set; } = new List<ClassifierRule>();
        [Nett.TomlIgnore]
        public List<ClassifierRule> ContentsClassifiers { get; set; } = new List<ClassifierRule>();

        public void PrepareClassifiers()
        {
            if (this.Classifiers.Count <= 0)
            {
                this.BuildDefaultClassifiers();
            }
        }

        private void BuildDefaultClassifiers()
        {
            this.Classifiers = new List<ClassifierRule>
            {
                new ClassifierRule()
                {
                    RuleOrder = 0,
                    RuleName = "DiscardShareEndsWtih",
                    EnumerationScope = EnumerationScope.ShareEnumeration,
                    MatchLocation = MatchLoc.ShareName,
                    MatchAction = MatchAction.Discard,
                    WordListType = MatchListType.EndsWith,
                    WordList = new List<string>()
                    {
                        // these are share names that make us skip the share instantly.
                        "\\print$",
                        "\\ipc$"
                    },
                },
                //new ClassifierRule()
                //{
                //    RuleOrder = 1,
                //    RuleName = "KeepCDollaShare",
                //    EnumerationScope = EnumerationScope.ShareEnumeration,
                //    MatchLocation = MatchLoc.ShareName,
                //    MatchAction = MatchAction.Snaffle,
                //    WordListType = MatchListType.EndsWith,
                //    WordList = new List<string>()
                //    {
                //        "\\C$"
                //    },
                //},
                new ClassifierRule()
                {
                    RuleOrder = 0,
                    RuleName = "DiscardFilepathContains",
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
                new ClassifierRule()
                {
                    RuleOrder = 0,
                    RuleName = "DiscardExtExact",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileExtension,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.Discard,
                    WordList = new List<string>()
                    {
                        // always skip these file extensions
                        // image formats
                        ".bmp",
                        ".eps",
                        ".gif",
                        ".ico",
                        ".jfi",
                        ".jfif",
                        ".jif",
                        ".jpe",
                        ".jpeg",
                        ".jpg",
                        ".png",
                        ".psd",
                        ".svg",
                        ".tif",
                        ".tiff",
                        ".webp",
                        ".xcf",
                        // fonts
                        ".ttf",
                        ".otf",
                        // misc
                        ".lock",
                        ".css",
                        ".less"
                    },
                },
                new ClassifierRule()
                {
                    RuleOrder = 1,
                    RuleName = "KeepExtExactBlack",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileExtension,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.Snaffle,
                    Triage = Triage.Black,
                    WordList = new List<string>()
                    {
                        // keepass databases
                        ".kdbx",
                        ".kdb",
                        // putty private keys
                        ".ppk",
                        // virtual disks
                        ".vmdk",
                        ".vhdx",
                        // virtual machines
                        ".ova",
                        ".ovf",
                        // password safe
                        ".psafe3",
                        // cloud service config
                        ".cscfg",
                        // kde wallet manager
                        ".kwallet",
                        // vpn profiles
                        ".tblk",
                        ".ovpn",
                        // db backups
                        ".mdf",
                        ".sdf",
                        ".sqldump"
                    },
                },
                new ClassifierRule()
                {
                    RuleOrder = 2,
                    RuleName = "KeepFilenameExactBlack",
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
                        "passwd",
                    },
                },
                new ClassifierRule()
                {
                    RuleOrder = 3,
                    RuleName = "KeepPathContainsBlack",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FilePath,
                    WordListType = MatchListType.Contains,
                    MatchAction = MatchAction.Snaffle,
                    Triage = Triage.Black,
                    WordList = new List<string>()
                    {
                        ".ssh\\",
                        ".purple\\accounts.xml",
                        ".aws\\",
                        ".gem\\credentials",
                        "doctl\\config.yaml",
                        "config\\hub"
                    },
                },
                new ClassifierRule()
                {
                    RuleOrder = 4,
                    RuleName = "KeepExtExactRed",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileExtension,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.Snaffle,
                    Triage = Triage.Red,
                    WordList = new List<string>()
                    {
                        // backups
                        ".bak",
                        // priv keys and certs
                        ".key",
                        ".pk12",
                        ".p12",
                        ".pkcs12",
                        //".pfx",
                        ".jks",
                        // rdp
                        ".rdp",
                        ".rdg",
                        // actionscript
                        ".asc",
                        // bitlocker recovery keys
                        ".bek",
                        // tpm backups
                        ".tpm",
                        ".fve",
                        // packet capture
                        ".pcap",
                        ".cap",
                        // misc key material
                        ".key",
                        ".keypair",
                        ".keychain",
                        // disk image
                        ".wim",
                    },
                },
                new ClassifierRule()
                {
                    RuleOrder = 5,
                    RuleName = "KeepFilenameExactRed",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileName,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.Snaffle,
                    Triage = Triage.Red,
                    WordList = new List<string>()
                    {
                        "unattend.xml",
                        ".netrc",
                        "_netrc",
                        ".htpasswd",
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
                        ".bash_history",
                        ".zsh_history",
                        ".sh_history",
                        "zhistory",
                        ".mysql_history",
                        ".psql_history",
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
                        ".dockercfg",
                        ".npmrc",
                        ".env",
                        ".bashrc",
                        ".profile",
                        ".zshrc",
                    },
                },
                new ClassifierRule()
                {
                    RuleOrder = 6,
                    RuleName = "KeepCertContainsPrivKeyRed",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileExtension,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.CheckForKeys,
                    Triage = Triage.Red,
                    WordList = new List<string>()
                    {
                        ".der",
                        ".pfx"
                    },
                },
                new ClassifierRule()
                {RuleOrder = 7,
                    RuleName = "GeneralGrepExtExact",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileExtension,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.Relay,
                    RelayTarget = "KeepGeneralGrepContainsYellow",
                    WordList = new List<string>()
                    {
                        ".txt",
                        ".sql",
                        ".log",
                        ".sqlite",
                        ".sqlite3",
                    },
                },
                new ClassifierRule()
                {
                    RuleName = "KeepGeneralGrepContainsYellow",
                    EnumerationScope = EnumerationScope.ContentsEnumeration,
                    MatchLocation = MatchLoc.FileContentAsString,
                    WordListType = MatchListType.Contains,
                    MatchAction = MatchAction.Snaffle,
                    Triage = Triage.Yellow,
                    WordList = new List<string>()
                    {
                        "password=",
                        "password =",
                        "cpassword",
                        "NVRAM config last updated"
                    },
                },
                
                new ClassifierRule()
                {
                    RuleOrder = 8,
                    RuleName = "CodeGrepExtExact",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileExtension,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.Relay,
                    RelayTarget = "KeepCodeGrepContainsRed",
                    WordList = new List<string>()
                    {
                        // python
                        ".py",
                        // php
                        ".php",
                        // asp.net
                        ".aspx",
                        ".ashx",
                        ".asmx",
                        ".asp",
                        ".cshtml",
                        ".cs",
                        ".ascx",
                        // java
                        ".jsp",
                        ".java",
                        // coldfusion
                        ".cfm",
                        // ruby
                        ".rb",
                    },
                },
                
                new ClassifierRule()
                {
                    RuleName = "KeepCodeGrepContainsRed",
                    EnumerationScope = EnumerationScope.ContentsEnumeration,
                    MatchLocation = MatchLoc.FileContentAsString,
                    WordListType = MatchListType.Contains,
                    MatchAction = MatchAction.Snaffle,
                    Triage = Triage.Red,
                    WordList = new List<string>()
                    {
                        "=jdbc:",
                        // thanks to graudit
                        "[Aa][Ww][Ss][_\\-\\.]?[Ss][Ee][Cc][Rr][Ee][Tt][_\\-\\.]?[Kk][Ee][Yy]",
                        "[Aa][Ww][Ss][_\\-\\.]?[Kk][Ee][Yy]",
                        "[_\\-\\.]?[Aa][Pp][Ii][_\\-\\.]?[Kk][Ee][Yy]",
                        "[_\\-\\.][Oo][Aa][Uu][Tt][Hh][[:space:]]*=",
                        "(client_secret|CLIENT_SECRET)",
                        "[Ss][Ee][Cc][Rr][Ee][Tt][_\\-\\.]?([Kk][Ee][Yy])?[[:space:]]*=",
                        "[_\\-\\.][Pp][Aa][Ss][Ss][Ww][Oo]?[Rr]?[Dd][[:space:]]*=[[:space:]]*[\'\"\\][^\'\"].....*",
                        "new OleDbConnection(", // asp.net
                        "\\.createConnection\\(.*",
                        "mysql_connect[[:space:]]*\\(.*\\$.*\\)",  // php
                        "mysql_pconnect[[:space:]]*\\(.*\\$.*\\)",  // php
                        "mysql_change_user[[:space:]]*\\(.*\\$.*\\)", // php
                        "pg_connect[[:space:]]*\\(.*\\$.*\\)", // php
                        "pg_pconnect[[:space:]]*\\(.*\\$.*\\)", // php
                        
                        // thanks to sshgit
                        "-----BEGIN [EC|RSA|DSA|OPENSSH] PRIVATE KEY----", // ssh priv key
                        "(A3T[A-Z0-9]|AKIA|AGPA|AROA|AIPA|ANPA|ANVA|ASIA)[A-Z0-9]{16}", // aws access key
                        "((\\\"|'|`)?((?i)aws)?_?((?i)account)_?((?i)id)?(\\\"|'|`)?\\\\s{0,50}(:|=>|=)\\\\s{0,50}(\\\"|'|`)[0-9]{4}-?[0-9]{4}-?[0-9]{4}(\\\"|'|`)?)", // aws account id
                        "((\\\"|'|`)?((?i)aws)?_?((?i)secret)_?((?i)access)?_?((?i)key)?_?((?i)id)?(\\\"|'|`)?\\\\s{0,50}(:|=>|=\\\\s{0,50}(\\\"|'|`)?[A-Za-z0-9/+=]{40}(\\\"|'|`)?)", // aws secret id
                        "((\\\"|'|`)?((?i)aws)?_?((?i)session)?_?((?i)token)?(\\\"|'|`)?\\\\s{0,50}(:|=>|=)\\\\s{0,50}(\\\"|'|`)[A-Za-z0-9/+=]{16,}(\\\"|'|`)?)", // aws session token
                        "(?i)artifactory.{0,50}(\\\"|'|`)?[a-zA-Z0-9=]{112}(\\\"|'|`)?",//Artifactory
                        "(?i)codeclima.{0,50}(\\\"|'|`)?[0-9a-f]{64}(\\\"|'|`)?",//CodeClimate
                        "EAACEdEose0cBA[0-9A-Za-z]+",//Facebook access token
                        "((\\\"|'|`)?type(\\\"|'|`)?\\\\s{0,50}(:|=>|=)\\\\s{0,50}(\\\"|'|`)?service_account(\\\"|'|`)?,?)",//Google (GCM) S"rvice account
                        "(?:r|s)k_[live|test]_[0-9a-zA-Z]{24}",//Stripe API key
                        "[0-9]+-[0-9A-Za-z_]{32}\\.apps\\.googleusercontent\\.com",//Google OAuth Key
                        "AIza[0-9A-Za-z\\-_]{35}",//Google Cloud API Key
                        "ya29\\.[0-9A-Za-z\\-_]+",//Google OAuth Access Token
                        "sk_[live|test]_[0-9a-z]{32}",//Picatic API key
                        "sq0atp-[0-9A-Za-z\\-_]{22}",//Square Access Token
                        "sq0csp-[0-9A-Za-z\\-_]{43}",//Square OAuth Secret
                        "access_token\\$production\\$[0-9a-z]{16}\\$[0-9a-f]{32}",//PayPal/Braintree Access Token
                        "amzn\\.mws\\.[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}",//Amazon MWS Auth Token
                        "SK[0-9a-fA-F]{32}",//Twilo API Key
                        "key-[0-9a-zA-Z]{32}",//MailGun API Key
                        "[0-9a-f]{32}-us[0-9]{12}",//MailChimp API Key
                        "sshpass -p.*['|\\\"]",//SSH Password
                        "(https\\://outlook\\.office.com/webhook/[0-9a-f-]{36}\\@)",//Outlook team
                        "(?i)sauce.{0,50}(\\\"|'|`)?[0-9a-f-]{36}(\\\"|'|`)?",//Sauce Token
                        "(xox[pboa]-[0-9]{12}-[0-9]{12}-[0-9]{12}-[a-z0-9]{32})",//Slack Token
                        "https://hooks.slack.com/services/T[a-zA-Z0-9_]{8}/B[a-zA-Z0-9_]{8}/[a-zA-Z0-9_]{24}",//Slack Webhook
                        "(?i)sonar.{0,50}(\\\"|'|`)?[0-9a-f]{40}(\\\"|'|`)?",//SonarQube Docs API Key
                        "(?i)hockey.{0,50}(\\\"|'|`)?[0-9a-f]{32}(\\\"|'|`)?",//HockeyApp
                        "([\\w+]{1,24})(://)([^$<]{1})([^\\s\";]{ 1,}):([^$<]{ 1})([^\\s\";]{1,})@[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\[a-zA-Z0-9()]{1,24}([\"\\s] +)",//Username and password in URI
                        "oy2[a-z0-9]{43}",//NuGet API Key
                        "[Aa][Ww][Ss][_\\-\\.]?[Ss][Ee][Cc][Rr][Ee][Tt][_\\-\\.]?[Kk][Ee][Yy]",
                        "[Aa][Ww][Ss][_\\-\\.]?[Kk][Ee][Yy]",
                        "[_\\-\\.]?[Aa][Pp][Ii][_\\-\\.]?[Kk][Ee][Yy]",
                        "[_\\-\\.][Oo][Aa][Uu][Tt][Hh][[:space:]]*=",
                        "(client_secret|CLIENT_SECRET)",
                        "[Ss][Ee][Cc][Rr][Ee][Tt][_\\-\\.]?([Kk][Ee][Yy])?[[:space:]]*=",
                        "[_\\-\\.][Pp][Aa][Ss][Ss][Ww][Oo]?[Rr]?[Dd][[:space:]]*=[[:space:]]*[\'\"\\][^\'\"].....*",
                        "-----BEGIN( RSA| OPENSSH| DSA| EC| PGP)? PRIVATE KEY( BLOCK)?-----",
                    },
                },
                
                new ClassifierRule()
                {
                    RuleOrder = 9,
                    RuleName = "ScriptGrepExtExact",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileExtension,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.Relay,
                    RelayTarget = "KeepScriptGrepContainsRed",
                    WordList = new List<string>()
                    {
                        // powershell
                        ".psd1",
                        ".psm1",
                        ".ps1",
                        // sh
                        ".sh",
                        // batch
                        ".bat",
                        // vbscript etc
                        ".wsf",
                        ".vbs",
                        // perl
                        ".pl",
                    },
                },
                new ClassifierRule()
                {
                    RuleName = "KeepScriptGrepContainsRed",
                    EnumerationScope = EnumerationScope.ContentsEnumeration,
                    MatchLocation = MatchLoc.FileContentAsString,
                    WordListType = MatchListType.Contains,
                    MatchAction = MatchAction.Snaffle,
                    Triage = Triage.Red,
                    WordList = new List<string>()
                    {
                        "get-credential",
                        "net user ",
                        "net localgroup ",
                        "psexec ",
                        "runas ",
                        " -Credential "
                    },
                },
                new ClassifierRule()
                {
                    RuleOrder = 10,
                    RuleName = "ConfigGrepExtExact",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileExtension,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.Relay,
                    RelayTarget = "KeepConfigGrepContainsRed",
                    WordList = new List<string>()
                    {
                        ".yaml",
                        ".xml",
                        ".json",
                        ".config",
                        ".ini",
                        ".inf",
                        ".cnf",
                        ".conf",
                    },
                },
                new ClassifierRule()
                {
                    RuleName = "KeepConfigGrepContainsRed",
                    EnumerationScope = EnumerationScope.ContentsEnumeration,
                    MatchLocation = MatchLoc.FileContentAsString,
                    WordListType = MatchListType.Contains,
                    MatchAction = MatchAction.Snaffle,
                    Triage = Triage.Red,
                    WordList = new List<string>()
                    {
                        "password=",
                        " connectionString=\"",
                        "sqlConnectionString=\"",
                        "validationKey=",
                        "decryptionKey=",
                        "NVRAM config last updated"
                    },
                },
                new ClassifierRule()
                {
                    RuleOrder = 11,
                    RuleName = "PrivKeyGrepNameContains",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileExtension,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.Relay,
                    RelayTarget = "KeepPrivKeyGrepContainsRed",
                    WordList = new List<string>()
                    {
                        // keys
                        "_rsa",
                        "_dsa",
                        "_ed25519",
                        "_ecdsa",
                        ".pem",
                    },
                },
                new ClassifierRule()
                {
                    RuleName = "KeepPrivKeyGrepContainsRed",
                    EnumerationScope = EnumerationScope.ContentsEnumeration,
                    MatchLocation = MatchLoc.FileContentAsString,
                    WordListType = MatchListType.Contains,
                    MatchAction = MatchAction.Snaffle,
                    Triage = Triage.Red,
                    WordList = new List<string>()
                    {
                        "PRIVATE KEY----",
                    },
                },
                /*
                new ClassifierRule()
                {
                    RuleOrder = 12,
                    RuleName = "KeepNameContainsGreen",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileName,
                    WordListType = MatchListType.Contains,
                    MatchAction = MatchAction.Snaffle,
                    Triage = Triage.Green,
                    WordList = new List<string>()
                    {
                        //magic words
                        "passw",
                        "as-built",
                        "handover",
                        "secret",
                        "thycotic",
                        "cyberark",
                    },
                },*/
            };
            /*
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
            */
        }
    }
}