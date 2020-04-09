using System.Collections.Generic;
using Classifiers;

namespace SnaffCore.Config
{
    public partial class Options
    {

        // Classifiers
        public List<Classifier> Classifiers { get; set; } = new List<Classifier>();
        public List<Classifier> ShareClassifiers { get; set; } = new List<Classifier>();
        public List<Classifier> DirClassifiers { get; set; } = new List<Classifier>();
        public List<Classifier> FileClassifiers { get; set; } = new List<Classifier>();
        public List<Classifier> ContentsClassifiers { get; set; } = new List<Classifier>();

        // classifier lists still needing to be classifier lists
        // NOT FOR LONG, SUCKA
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

        [Nett.TomlIgnore]
        public string[] GrepStrings { get; set; } =
        {
            // these are strings that make a file interesting if found within.
            "net user ",
            "net localgroup ",
            "psexec ",
            "runas ",
            " -Credential ",
            " -AsSecureString",
            //"jdbc",
            //"odbc",
            "password",
            "PRIVATE KEY----",
            " connectionString=\"",
            "sqlConnectionString=\"",
            "validationKey=",
            "decryptionKey=",
            //"credential",
            //"root:",
            //"admin:",
            //"login",
            "cpassword"
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

        */
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
                        "ipc$",
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
                    MatchAction = MatchAction.Relay,
                    RelayTarget = "", //TODO
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