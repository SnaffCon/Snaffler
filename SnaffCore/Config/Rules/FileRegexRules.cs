using System.Collections.Generic;
using Classifiers;

namespace SnaffCore.Config
{
    public partial class Options
    {
        private void BuildFileRegexClassifiers()
        {
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be subjected to a generic search for keys and such.",
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
            });

            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with contents matching these regexen are very interesting.",
                RuleName = "KeepCodeGrepContainsRed",
                EnumerationScope = EnumerationScope.ContentsEnumeration,
                MatchLocation = MatchLoc.FileContentAsString,
                WordListType = MatchListType.Regex,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Red,
                WordList = new List<string>()
                    {
                        // ruby
                        "DBI\\.connect\\(",
                        // perl
                        "DBI\\-\\>connect\\(",
                        // java
                        "\\.getConnection(\\\"jdbc\\:",

                        // need to add .sql file stuff where users being added
                        // CREATE USER 'newuser'@'localhost' IDENTIFIED BY 'user_password'; // mysql
                        // CREATE USER books_admin IDENTIFIED BY MyPassword; // oracle
                        // CREATE USER tom WITH PASSWORD 'myPassword'; // postgresql
                        // CREATE USER Mary WITH PASSWORD = '********'; //mssql
                        // CREATE LOGIN ASQLLogin WITH PASSWORD = 0x0200C6FAAFFE9C6BAA377C6E74DF8FC9819860E54B31EAE9F4D326D10B8707C36F030DE5826577B676E2F8CB02FDB31BD829691FD55E1C616F87122D926B9C27FB13356A63D6 HASHED; // mssql

                        // thanks to graudit
                        "[Aa][Ww][Ss][_\\-\\.]?[Ss][Ee][Cc][Rr][Ee][Tt][_\\-\\.]?[Kk][Ee][Yy]",
                        "[Aa][Ww][Ss][_\\-\\.]?[Kk][Ee][Yy]",
                        "[_\\-\\.]?[Aa][Pp][Ii][_\\-\\.]?[Kk][Ee][Yy]",
                        "[_\\-\\.][Oo][Aa][Uu][Tt][Hh][[:space:]]*=",
                        "(client_secret|CLIENT_SECRET)",
                        "[Ss][Ee][Cc][Rr][Ee][Tt][_\\-\\.]?([Kk][Ee][Yy])?[[:space:]]*=",
                        "[_\\-\\.][Pp][Aa][Ss][Ss][Ww][Oo]?[Rr]?[Dd][[:space:]]*=[[:space:]]*[\'\"\\][^\'\"].....*",

                        "-----BEGIN( RSA| OPENSSH| DSA| EC| PGP)? PRIVATE KEY( BLOCK)?-----",

                        "new OleDbConnection\\(", // asp.net
                        "\\.createConnection\\(.*", // asp.net

                        "mysql\\.connector\\.connect\\(", //python
                        "psycopg2\\.connect\\(", // python postgres

                        "mysql_connect[[:space:]]*\\(.*\\$.*\\)",  // php
                        "mysql_pconnect[[:space:]]*\\(.*\\$.*\\)",  // php
                        "mysql_change_user[[:space:]]*\\(.*\\$.*\\)", // php
                        "pg_connect[[:space:]]*\\(.*\\$.*\\)", // php
                        "pg_pconnect[[:space:]]*\\(.*\\$.*\\)", // php
                        
                        // bash etc
                        "sshpass -p.*['|\\\"]",//SSH Password

                        // api keys
                        "-----BEGIN [EC|RSA|DSA|OPENSSH] PRIVATE KEY----", // priv key
                        "(A3T[A-Z0-9]|AKIA|AGPA|AROA|AIPA|ANPA|ANVA|ASIA)[A-Z0-9]{16}", // aws access key
                        "((\\\"|'|`)?((?i)aws)?_?((?i)account)_?((?i)id)?(\\\"|'|`)?\\\\s{0,50}(:|=>|=)\\\\s{0,50}(\\\"|'|`)[0-9]{4}-?[0-9]{4}-?[0-9]{4}(\\\"|'|`)?)", // aws account id
                        "((\\\"|'|`)?((?i)aws)?_?((?i)secret)_?((?i)access)?_?((?i)key)?_?((?i)id)?(\\\"|'|`)?\\\\s{0,50}(:|=>|=)\\\\s{0,50}(\\\"|'|`)?[A-Za-z0-9/+=]{40}(\\\"|'|`)?)", // aws secret id
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
                        "(https\\://outlook\\.office.com/webhook/[0-9a-f-]{36}\\@)",//Outlook team
                        "(?i)sauce.{0,50}(\\\"|'|`)?[0-9a-f-]{36}(\\\"|'|`)?",//Sauce Token
                        "(xox[pboa]-[0-9]{12}-[0-9]{12}-[0-9]{12}-[a-z0-9]{32})",//Slack Token
                        "https://hooks.slack.com/services/T[a-zA-Z0-9_]{8}/B[a-zA-Z0-9_]{8}/[a-zA-Z0-9_]{24}",//Slack Webhook
                        "(?i)sonar.{0,50}(\\\"|'|`)?[0-9a-f]{40}(\\\"|'|`)?",//SonarQube Docs API Key
                        "(?i)hockey.{0,50}(\\\"|'|`)?[0-9a-f]{32}(\\\"|'|`)?",//HockeyApp
                        //"([\\w+]{1,24})(://)([^$<]{1})([^\\s\";]{ 1,}):([^$<]{ 1})([^\\s\";]{1,})@[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\[a-zA-Z0-9()]{1,24}([\"\\s] +)",//Username and password in URI
                        "oy2[a-z0-9]{43}",//NuGet API Key
                    },
            });
            
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be subjected to a generic search for keys and such.",
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
            });
            
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Script files with contents matching these regexen are very interesting.",
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
            });
        } 
    }
}
