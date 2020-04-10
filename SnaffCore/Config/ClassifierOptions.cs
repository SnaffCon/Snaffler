using System.Collections.Generic;
using Classifiers;

namespace SnaffCore.Config
{
    public partial class Options
    {
        // Classifiers
        public List<Classifier> Classifiers { get; set; } = new List<Classifier>();
        [Nett.TomlIgnore]
        public List<Classifier> ShareClassifiers { get; set; } = new List<Classifier>();
        [Nett.TomlIgnore]
        public List<Classifier> DirClassifiers { get; set; } = new List<Classifier>();
        [Nett.TomlIgnore]
        public List<Classifier> FileClassifiers { get; set; } = new List<Classifier>();
        [Nett.TomlIgnore]
        public List<Classifier> ContentsClassifiers { get; set; } = new List<Classifier>();

        public void PrepareClassifiers()
        {
            if (this.Classifiers.Count <= 0)
            {
                this.BuildDefaultClassifiers();
            }
        }

        private void BuildDefaultClassifiers()
        {
            // TODO need to add a default classifier to handle C$ shares

            this.Classifiers = new List<Classifier>
            {
                new Classifier()
                {
                    ClassifierName = "DiscardShareExact",
                    EnumerationScope = EnumerationScope.ShareEnumeration,
                    MatchLocation = MatchLoc.ShareName,
                    MatchAction = MatchAction.Discard,
                    WordListType = MatchListType.EndsWith,
                    WordList = new List<string>()
                    {
                        // these are share names that make us skip the share instantly.
                        "print$",
                        "ipc$"
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
                    ClassifierName = "DiscardExtExact",
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
                new Classifier()
                {
                    ClassifierName = "KeepExtExactBlack",
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
                        ".wim",
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
                new Classifier()
                {
                    ClassifierName = "KeepExtExactRed",
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
                        // misc key material
                        ".key",
                        ".keypair",
                        ".keychain",
                    },
                },
                /*
                new Classifier()
                {
                    ClassifierName = "KeepNameContainsGreen",
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
                },
                */
                
                new Classifier()
                {
                    ClassifierName = "KeepPathContainsRed",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FilePath,
                    WordListType = MatchListType.Contains,
                    MatchAction = MatchAction.Snaffle,
                    Triage = Triage.Red,
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
                new Classifier()
                {
                    ClassifierName = "KeepFilenameExactBlack",
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
                    ClassifierName = "KeepCertContainsPrivKeyRed",
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
                new Classifier()
                {
                    ClassifierName = "GeneralGrepExtExact",
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
                new Classifier()
                {
                    ClassifierName = "KeepGeneralGrepContainsYellow",
                    EnumerationScope = EnumerationScope.ContentsEnumeration,
                    MatchLocation = MatchLoc.FileContentAsString,
                    WordListType = MatchListType.Contains,
                    MatchAction = MatchAction.Snaffle,
                    Triage = Triage.Yellow,
                    WordList = new List<string>()
                    {
                        "password=",
                        "password =",
                        "cpassword"
                    },
                },
                /*
                new Classifier()
                {
                    ClassifierName = "CodeGrepExtExact",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileExtension,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.Relay,
                    RelayTarget = "KeepCodeGrepContainsYellow",
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
                
                new Classifier()
                {
                    ClassifierName = "KeepCodeGrepContainsYellow",
                    EnumerationScope = EnumerationScope.ContentsEnumeration,
                    MatchLocation = MatchLoc.FileContentAsString,
                    WordListType = MatchListType.Contains,
                    MatchAction = MatchAction.Snaffle,
                    Triage = Triage.Yellow,
                    WordList = new List<string>()
                    {
                        "password",
                    },
                },*/
                
                new Classifier()
                {
                    ClassifierName = "ScriptGrepExtExact",
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
                new Classifier()
                {
                    ClassifierName = "KeepScriptGrepContainsRed",
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
                new Classifier()
                {
                    ClassifierName = "ConfigGrepExtExact",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileExtension,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.Relay,
                    RelayTarget = "KeepConfigGrepContainsYellow",
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
                new Classifier()
                {
                    ClassifierName = "KeepConfigGrepContainsYellow",
                    EnumerationScope = EnumerationScope.ContentsEnumeration,
                    MatchLocation = MatchLoc.FileContentAsString,
                    WordListType = MatchListType.Contains,
                    MatchAction = MatchAction.Snaffle,
                    Triage = Triage.Yellow,
                    WordList = new List<string>()
                    {
                        "password=",
                        " connectionString=\"",
                        "sqlConnectionString=\"",
                        "validationKey=",
                        "decryptionKey=",
                    },
                },
                new Classifier()
                {
                    ClassifierName = "PrivKeyGrepNameContains",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileExtension,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.Relay,
                    RelayTarget = "KeepConfigGrepContainsYellow",
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
                new Classifier()
                {
                    ClassifierName = "KeepPrivKeyGrepContainsRed",
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
            };
            // regexes

            /*public class Regexes
            {

                Regex privkey = new Regex(@"-----BEGIN [EC|RSA|DSA|OPENSSH] PRIVATE KEY----", 
                    RegexOptions.Compiled);

                Regex awsaccesskey = new Regex(@"(A3T[A-Z0-9]|AKIA|AGPA|AROA|AIPA|ANPA|ANVA|ASIA)[A-Z0-9]{16}",
                    RegexOptions.Compiled);

                Regex awsaccountid = new Regex(@"((\\\"|'|`)?((?i)aws)?_?((?i)account)_?((?i)id)?(\\\"|'|`)?\\\\s{0,50}(:|=>|=)\\\\s{0,50}(\\\"|'|`)?[0-9]{4}-?[0-9]{4}-?[0-9]{4}(\\\"|'|`)?)",
                    RegexOptions.Compiled);

                Regex awssecretkey = new Regex(@"((\\\"|'|`)?((?i)aws)?_?((?i)secret)_?((?i)access)?_?((?i)key)?_?((?i)id)?(\\\"|'|`)?\\\\s{0,50}(:|=>|=)\\\\s{0,50}(\\\"|'|`)?[A-Za-z0-9/+=]{40}(\\\"|'|`)?)",
                    RegexOptions.Compiled);
                /*

            - part: 'contents'
            regex: "((\\\"|'|`)?((?i)aws)?_?((?i)access)_?((?i)key)?_?((?i)id)?(\\\"|'|`)?\\\\s{0,50}(:|=>|=)\\\\s{0,50}(\\\"|'|`)?(A3T[A-Z0-9]|AKIA|AGPA|AIDA|AROA|AIPA|ANPA|ANVA|ASIA)[A-Z0-9]{16}(\\\"|'|`)?)"
            name: 'AWS Access Key ID'
            - part: 'contents'
            regex: "((\\\"|'|`)?((?i)aws)?_?((?i)account)_?((?i)id)?(\\\"|'|`)?\\\\s{0,50}(:|=>|=)\\\\s{0,50}(\\\"|'|`)?[0-9]{4}-?[0-9]{4}-?[0-9]{4}(\\\"|'|`)?)"
            name: 'AWS Account ID'
            - part: 'contents'
            regex: "((\\\"|'|`)?((?i)aws)?_?((?i)secret)_?((?i)access)?_?((?i)key)?_?((?i)id)?(\\\"|'|`)?\\\\s{0,50}(:|=>|=)\\\\s{0,50}(\\\"|'|`)?[A-Za-z0-9/+=]{40}(\\\"|'|`)?)"
            name: 'AWS Secret Access Key'
            - part: 'contents'
            regex: "((\\\"|'|`)?((?i)aws)?_?((?i)session)?_?((?i)token)?(\\\"|'|`)?\\\\s{0,50}(:|=>|=)\\\\s{0,50}(\\\"|'|`)?[A-Za-z0-9/+=]{16,}(\\\"|'|`)?)"
            name: 'AWS Session Token'
            - part: 'contents'
            regex: "(?i)artifactory.{0,50}(\\\"|'|`)?[a-zA-Z0-9=]{112}(\\\"|'|`)?"
            name: 'Artifactory'
            - part: 'contents'
            regex: "(?i)codeclima.{0,50}(\\\"|'|`)?[0-9a-f]{64}(\\\"|'|`)?"
            name: 'CodeClimate'
            - part:  'contents'
            regex: 'EAACEdEose0cBA[0-9A-Za-z]+'
            name: 'Facebook access token'
            - part: 'contents'
            regex: "((\\\"|'|`)?type(\\\"|'|`)?\\\\s{0,50}(:|=>|=)\\\\s{0,50}(\\\"|'|`)?service_account(\\\"|'|`)?,?)"
            name: 'Google (GCM) Service account'
            - part:  'contents'
            regex: '(?:r|s)k_[live|test]_[0-9a-zA-Z]{24}'
            name: 'Stripe API key'
            - part:  'contents'
            regex: '[0-9]+-[0-9A-Za-z_]{32}\.apps\.googleusercontent\.com'
            name: 'Google OAuth Key'
            - part: 'contents'
            regex: 'AIza[0-9A-Za-z\\-_]{35}'
            name: 'Google Cloud API Key'
            - part: 'contents'
            regex: 'ya29\\.[0-9A-Za-z\\-_]+'
            name: 'Google OAuth Access Token'
            - part:  'contents'
            regex: 'sk_[live|test]_[0-9a-z]{32}'
            name: 'Picatic API key'
            - part:  'contents'
            regex: 'sq0atp-[0-9A-Za-z\-_]{22}'
            name: 'Square Access Token'
            - part:  'contents'
            regex: 'sq0csp-[0-9A-Za-z\-_]{43}'
            name: 'Square OAuth Secret'
            - part:  'contents'
            regex: 'access_token\$production\$[0-9a-z]{16}\$[0-9a-f]{32}'
            name: 'PayPal/Braintree Access Token'
            - part:  'contents'
            regex: 'amzn\.mws\.[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}'
            name: 'Amazon MWS Auth Token'
            - part:  'contents'
            regex: 'SK[0-9a-fA-F]{32}'
            name: 'Twilo API Key'
            - part:  'contents'
            regex: 'key-[0-9a-zA-Z]{32}'
            name: 'MailGun API Key'
            - part:  'contents'
            regex: '[0-9a-f]{32}-us[0-9]{12}'
            name: 'MailChimp API Key'
            - part:  'contents'
            regex: "sshpass -p.*['|\\\"]"
            name: 'SSH Password'
            - part: 'contents'
            regex: '(https\\://outlook\\.office.com/webhook/[0-9a-f-]{36}\\@)'
            name: 'Outlook team'
            - part: 'contents'
            regex: "(?i)sauce.{0,50}(\\\"|'|`)?[0-9a-f-]{36}(\\\"|'|`)?"
            name: 'Sauce Token'
            - part: 'contents'
            regex: '(xox[pboa]-[0-9]{12}-[0-9]{12}-[0-9]{12}-[a-z0-9]{32})'
            name: 'Slack Token'
            - part: 'contents'
            regex: 'https://hooks.slack.com/services/T[a-zA-Z0-9_]{8}/B[a-zA-Z0-9_]{8}/[a-zA-Z0-9_]{24}'
            name: 'Slack Webhook'
            - part: 'contents'
            regex: "(?i)sonar.{0,50}(\\\"|'|`)?[0-9a-f]{40}(\\\"|'|`)?"
            name: 'SonarQube Docs API Key'
            - part: 'contents'
            regex: "(?i)hockey.{0,50}(\\\"|'|`)?[0-9a-f]{32}(\\\"|'|`)?"
            name: 'HockeyApp'
            - part: 'contents'
            regex: '([\w+]{1,24})(://)([^$<]{1})([^\s";]{1,}):([^$<]{1})([^\s";]{1,})@[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,24}([^\s]+)'
            name: 'Username and password in URI'
            - part: 'contents'
            regex: 'oy2[a-z0-9]{43}'
            name: 'NuGet API Key'


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
            */
        }
    }
}