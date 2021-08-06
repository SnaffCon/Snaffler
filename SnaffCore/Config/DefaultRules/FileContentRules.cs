using Classifiers;
using System.Collections.Generic;

namespace SnaffCore.Config
{
    public partial class Options
    {
        private void BuildFileContentRules()
        {

            /*
            // TEST RULE for doc parsing
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be parsed as part of a test.",
                RuleName = "DocParseTest",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepDocRegexGreen",
                WordList = new List<string>()
                {
                    ".doc",".docx",".xls",".xlsx",".eml",".msg",".pdf",".ppt",".rtf"
                },
            });

            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with contents matching these regexen are kind of interesting for purposes of this test.",
                RuleName = "KeepDocRegexGreen",
                EnumerationScope = EnumerationScope.ContentsEnumeration,
                MatchLocation = MatchLoc.FileContentAsString,
                WordListType = MatchListType.Regex,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Green,
                WordList = new List<string>()
                {
                    "password",
                    "prepared",
                    "security"
                }
            });

            */

            // Python
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be searched for python related strings.",
                RuleName = "PyContentByExt",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepPyRegexRed",
                WordList = new List<string>()
                {
                    // python
                    "\\.py"
                },
            });

            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with contents matching these regexen are very interesting.",
                RuleName = "KeepPyRegexRed",
                EnumerationScope = EnumerationScope.ContentsEnumeration,
                MatchLocation = MatchLoc.FileContentAsString,
                WordListType = MatchListType.Regex,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Red,
                WordList = new List<string>()
                {
                    // python
                    "mysql\\.connector\\.connect\\(", //python
                    "psycopg2\\.connect\\(", // python postgres
                    // generic tokens etc, same for most languages.
                    "passwo?r?d\\s*=\\s*[\\\'\\\"][^\\\'\\\"]....",
                    "CREATE (USER|LOGIN) .{0,200} (IDENTIFIED BY|WITH PASSWORD)", // sql creds
                    "(xox[pboa]-[0-9]{12}-[0-9]{12}-[0-9]{12}-[a-z0-9]{32})", //Slack Token
                    "https://hooks.slack.com/services/T[a-zA-Z0-9_]{8}/B[a-zA-Z0-9_]{8}/[a-zA-Z0-9_]{24}", //Slack Webhook
                    "aws[_\\-\\.]?key", // aws mnagic
                    "[_\\-\\.]?api[_\\-\\.]?key", // stuff
                    "[_\\-\\.]oauth\\s*=", // oauth stuff
                    "client_secret", // fun
                    "secret[_\\-\\.]?(key)?\\s*=",
                    "-----BEGIN( RSA| OPENSSH| DSA| EC| PGP)? PRIVATE KEY( BLOCK)?-----",
                    "(\\s|\\\'|\\\"|\\^|=)(A3T[A-Z0-9]|AKIA|AGPA|AROA|AIPA|ANPA|ANVA|ASIA)[A-Z0-9]{16}(\\s|\\\'|\\\"|$)", // aws access key
                }
            });
            // PHP
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be searched for php related strings.",
                RuleName = "phpContentByExt",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepPhpRegexRed",
                WordList = new List<string>()
                {
                    // php
                    "\\.php",
                    "\\.phtml",
                    "\\.inc",
                    "\\.php3",
                    "\\.php5",
                    "\\.php7"
                },
            });
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with contents matching these regexen are very interesting.",
                RuleName = "KeepPhpRegexRed",
                EnumerationScope = EnumerationScope.ContentsEnumeration,
                MatchLocation = MatchLoc.FileContentAsString,
                WordListType = MatchListType.Regex,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Red,
                WordList = new List<string>()
                {
                    // php
                    "mysql_connect\\s*\\(.*\\$.*\\)", // php
                    "mysql_pconnect\\s*\\(.*\\$.*\\)", // php
                    "mysql_change_user\\s*\\(.*\\$.*\\)", // php
                    "pg_connect\\s*\\(.*\\$.*\\)", // php
                    "pg_pconnect\\s*\\(.*\\$.*\\)", // php
                    // generic tokens etc, same for most languages.
                    "passwo?r?d\\s*=\\s*[\\\'\\\"][^\\\'\\\"]....",
                    "CREATE (USER|LOGIN) .{0,200} (IDENTIFIED BY|WITH PASSWORD)", // sql creds
                    "(xox[pboa]-[0-9]{12}-[0-9]{12}-[0-9]{12}-[a-z0-9]{32})", //Slack Token
                    "https://hooks.slack.com/services/T[a-zA-Z0-9_]{8}/B[a-zA-Z0-9_]{8}/[a-zA-Z0-9_]{24}", //Slack Webhook
                    "aws[_\\-\\.]?key", // aws mnagic
                    "[_\\-\\.]?api[_\\-\\.]?key", // stuff
                    "[_\\-\\.]oauth\\s*=", // oauth stuff
                    "client_secret", // fun
                    "secret[_\\-\\.]?(key)?\\s*=",
                    "-----BEGIN( RSA| OPENSSH| DSA| EC| PGP)? PRIVATE KEY( BLOCK)?-----",
                    "(\\s|\\\'|\\\"|\\^|=)(A3T[A-Z0-9]|AKIA|AGPA|AROA|AIPA|ANPA|ANVA|ASIA)[A-Z0-9]{16}(\\s|\\\'|\\\"|$)", // aws access key
                }
            });
            // CSharp
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be searched for CSharp and ASP.NET related strings.",
                RuleName = "csContentByExt",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepCsRegexRed",
                WordList = new List<string>()
                {
                    // asp.net
                    "\\.aspx",
                    "\\.ashx",
                    "\\.asmx",
                    "\\.asp",
                    "\\.cshtml",
                    "\\.cs",
                    "\\.ascx"
                },
            });
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with contents matching these regexen are very interesting.",
                RuleName = "KeepCsRegexRed",
                EnumerationScope = EnumerationScope.ContentsEnumeration,
                MatchLocation = MatchLoc.FileContentAsString,
                WordListType = MatchListType.Regex,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Red,
                WordList = new List<string>()
                {
                    // csharp
                    "connectionstring.{1,200}passw",
                    "validationkey\\s*=\\s*[\\\'\\\"][^\\\'\\\"]....",
                    "decryptionkey\\s*=\\s*[\\\'\\\"][^\\\'\\\"]....",
                    "passwo?r?d\\s*=\\s*[\\\'\\\"][^\\\'\\\"]....",
                    "CREATE (USER|LOGIN) .{0,200} (IDENTIFIED BY|WITH PASSWORD)", // sql creds
                    "(xox[pboa]-[0-9]{12}-[0-9]{12}-[0-9]{12}-[a-z0-9]{32})", //Slack Token
                    "https://hooks.slack.com/services/T[a-zA-Z0-9_]{8}/B[a-zA-Z0-9_]{8}/[a-zA-Z0-9_]{24}", //Slack Webhook
                    "aws[_\\-\\.]?key", // aws mnagic
                    "[_\\-\\.]?api[_\\-\\.]?key", // stuff
                    "[_\\-\\.]oauth\\s*=", // oauth stuff
                    "client_secret", // fun
                    "secret[_\\-\\.]?(key)?\\s*=",
                    "-----BEGIN( RSA| OPENSSH| DSA| EC| PGP)? PRIVATE KEY( BLOCK)?-----",
                    "(\\s|\\\'|\\\"|\\^|=)(A3T[A-Z0-9]|AKIA|AGPA|AROA|AIPA|ANPA|ANVA|ASIA)[A-Z0-9]{16}(\\s|\\\'|\\\"|$)", // aws access key
                }
            });
            //Java
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be searched for Java and ColdFusion related strings.",
                RuleName = "javaContentByExt",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepJavaRegexRed",
                WordList = new List<string>()
                {
                    // java
                    "\\.jsp",
                    "\\.do",
                    "\\.java",
                    // coldfusion
                    "\\.cfm",
                },
            });
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with contents matching these regexen are very interesting.",
                RuleName = "KeepJavaRegexRed",
                EnumerationScope = EnumerationScope.ContentsEnumeration,
                MatchLocation = MatchLoc.FileContentAsString,
                WordListType = MatchListType.Regex,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Red,
                WordList = new List<string>()
                {
                    // java
                    "\\.getConnection\\(\\\"jdbc\\:",
                    // generic tokens etc, same for most languages.
                    "passwo?r?d\\s*=\\s*[\\\'\\\"][^\\\'\\\"]....",
                    "CREATE (USER|LOGIN) .{0,200} (IDENTIFIED BY|WITH PASSWORD)", // sql creds
                    "(xox[pboa]-[0-9]{12}-[0-9]{12}-[0-9]{12}-[a-z0-9]{32})", //Slack Token
                    "https://hooks.slack.com/services/T[a-zA-Z0-9_]{8}/B[a-zA-Z0-9_]{8}/[a-zA-Z0-9_]{24}", //Slack Webhook
                    "aws[_\\-\\.]?key", // aws mnagic
                    "[_\\-\\.]?api[_\\-\\.]?key", // stuff
                    "[_\\-\\.]oauth\\s*=", // oauth stuff
                    "client_secret", // fun
                    "secret[_\\-\\.]?(key)?\\s*=",
                    "-----BEGIN( RSA| OPENSSH| DSA| EC| PGP)? PRIVATE KEY( BLOCK)?-----",
                    "(\\s|\\\'|\\\"|\\^|=)(A3T[A-Z0-9]|AKIA|AGPA|AROA|AIPA|ANPA|ANVA|ASIA)[A-Z0-9]{16}(\\s|\\\'|\\\"|$)", // aws access key
                }
            });
            // Ruby
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be searched for Rubby related strings.",
                RuleName = "rubyContentByExt",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepRubyRegexRed",
                WordList = new List<string>()
                {
                    // ruby
                    "\\.rb"
                },
            });
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with contents matching these regexen are very interesting.",
                RuleName = "KeepRubyRegexRed",
                EnumerationScope = EnumerationScope.ContentsEnumeration,
                MatchLocation = MatchLoc.FileContentAsString,
                WordListType = MatchListType.Regex,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Red,
                WordList = new List<string>()
                {
                    // ruby
                    "DBI\\.connect\\(",
                    // generic tokens etc, same for most languages.
                    "passwo?r?d\\s*=\\s*[\\\'\\\"][^\\\'\\\"]....",
                    "CREATE (USER|LOGIN) .{0,200} (IDENTIFIED BY|WITH PASSWORD)", // sql creds
                    "(xox[pboa]-[0-9]{12}-[0-9]{12}-[0-9]{12}-[a-z0-9]{32})", //Slack Token
                    "https://hooks.slack.com/services/T[a-zA-Z0-9_]{8}/B[a-zA-Z0-9_]{8}/[a-zA-Z0-9_]{24}", //Slack Webhook
                    "aws[_\\-\\.]?key", // aws mnagic
                    "[_\\-\\.]?api[_\\-\\.]?key", // stuff
                    "[_\\-\\.]oauth\\s*=", // oauth stuff
                    "client_secret", // fun
                    "secret[_\\-\\.]?(key)?\\s*=",
                    "-----BEGIN( RSA| OPENSSH| DSA| EC| PGP)? PRIVATE KEY( BLOCK)?-----",
                    "(\\s|\\\'|\\\"|\\^|=)(A3T[A-Z0-9]|AKIA|AGPA|AROA|AIPA|ANPA|ANVA|ASIA)[A-Z0-9]{16}(\\s|\\\'|\\\"|$)", // aws access key
                }
            });

            // Perl
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be searched for Perl related strings.",
                RuleName = "perlContentByExt",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepPerlRegexRed",
                WordList = new List<string>()
                {
                    // perl
                    "\\.pl"
                },
            });
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with contents matching these regexen are very interesting.",
                RuleName = "KeepPerlRegexRed",
                EnumerationScope = EnumerationScope.ContentsEnumeration,
                MatchLocation = MatchLoc.FileContentAsString,
                WordListType = MatchListType.Regex,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Red,
                WordList = new List<string>()
                {
                    // perl
                    "DBI\\-\\>connect\\(",
                    // generic tokens etc, same for most languages.
                    "passwo?r?d\\s*=\\s*[\\\'\\\"][^\\\'\\\"]....",
                    "CREATE (USER|LOGIN) .{0,200} (IDENTIFIED BY|WITH PASSWORD)", // sql creds
                    "(xox[pboa]-[0-9]{12}-[0-9]{12}-[0-9]{12}-[a-z0-9]{32})", //Slack Token
                    "https://hooks.slack.com/services/T[a-zA-Z0-9_]{8}/B[a-zA-Z0-9_]{8}/[a-zA-Z0-9_]{24}", //Slack Webhook
                    "aws[_\\-\\.]?key", // aws mnagic
                    "[_\\-\\.]?api[_\\-\\.]?key", // stuff
                    "[_\\-\\.]oauth\\s*=", // oauth stuff
                    "client_secret", // fun
                    "secret[_\\-\\.]?(key)?\\s*=",
                    "-----BEGIN( RSA| OPENSSH| DSA| EC| PGP)? PRIVATE KEY( BLOCK)?-----",
                    "(\\s|\\\'|\\\"|\\^|=)(A3T[A-Z0-9]|AKIA|AGPA|AROA|AIPA|ANPA|ANVA|ASIA)[A-Z0-9]{16}(\\s|\\\'|\\\"|$)", // aws access key
                }
            });

            // PowerShell
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be searched for PowerShell related strings.",
                RuleName = "psContentByExt",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepPsRegexRed",
                WordList = new List<string>()
                {
                    // powershell
                    "\\.psd1",
                    "\\.psm1",
                    "\\.ps1",
                },
            });
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with contents matching these regexen are very interesting.",
                RuleName = "KeepPsRegexRed",
                EnumerationScope = EnumerationScope.ContentsEnumeration,
                MatchLocation = MatchLoc.FileContentAsString,
                WordListType = MatchListType.Regex,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Red,
                WordList = new List<string>()
                {
                    // PS
                    "net user ",
                    "psexec .{0,100} -p ",
                    "net use .{0,300} /user:",
                    "-SecureString",
                    "-AsPlainText",
                    "\\[Net.NetworkCredential\\]::new\\(",
                    // generic tokens etc, same for most languages.
                    "passwo?r?d\\s*=\\s*[\\\'\\\"][^\\\'\\\"]....",
                    "CREATE (USER|LOGIN) .{0,00} (IDENTIFIED BY|WITH PASSWORD)", // sql creds
                    "(xox[pboa]-[0-9]{12}-[0-9]{12}-[0-9]{12}-[a-z0-9]{32})", //Slack Token
                    "https://hooks.slack.com/services/T[a-zA-Z0-9_]{8}/B[a-zA-Z0-9_]{8}/[a-zA-Z0-9_]{24}", //Slack Webhook
                    "aws[_\\-\\.]?key", // aws mnagic
                    "[_\\-\\.]?api[_\\-\\.]?key", // stuff
                    "[_\\-\\.]oauth\\s*=", // oauth stuff
                    "client_secret", // fun
                    "secret[_\\-\\.]?(key)?\\s*=",
                    "-----BEGIN( RSA| OPENSSH| DSA| EC| PGP)? PRIVATE KEY( BLOCK)?-----",
                    "(\\s|\\\'|\\\"|\\^|=)(A3T[A-Z0-9]|AKIA|AGPA|AROA|AIPA|ANPA|ANVA|ASIA)[A-Z0-9]{16}(\\s|\\\'|\\\"|$)", // aws access key
                }
            });

            // Batch
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be searched for cmd.exe/batch file related strings.",
                RuleName = "cmdContentByExt",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepCmdRegexRed",
                WordList = new List<string>()
                {
                    // cmd.exe
                    "\\.bat",
                    "\\.cmd"
                },
            });
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with contents matching these regexen are very interesting.",
                RuleName = "KeepCmdRegexRed",
                EnumerationScope = EnumerationScope.ContentsEnumeration,
                MatchLocation = MatchLoc.FileContentAsString,
                WordListType = MatchListType.Regex,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Red,
                WordList = new List<string>()
                {
                    // cmd
                    // password variable in bat file
                    "passwo?r?d\\s*=\\s*[\\\'\\\"][^\\\'\\\"]....",
                    // creation of scheduled tasks with password
                    "schtasks.{1,300}(/rp\\s|/p\\s)",
                    // looking for net use or net user commands since these can contain credentials
                    "net user ",
                    "psexec .{0,100} -p ",
                    "net use .{0,300} /user:"
                }
            });

            // bash/sh/zsh/etc
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be searched for Bash related strings.",
                RuleName = "bashContentByExt",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepBashRegexRed",
                WordList = new List<string>()
                {
                    // bash, sh, zsh, etc
                    "\\.sh",
                    "\\.rc",
                    "\\.profile"
                },
            });
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with contents matching these regexen are very interesting.",
                RuleName = "KeepBashRegexRed",
                EnumerationScope = EnumerationScope.ContentsEnumeration,
                MatchLocation = MatchLoc.FileContentAsString,
                WordListType = MatchListType.Regex,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Red,
                WordList = new List<string>()
                {
                    // bash
                    "sshpass.{1,300}-p", //SSH Password
                    // generic tokens etc, same for most languages.
                    "passwo?r?d\\s*=\\s*[\\\'\\\"][^\\\'\\\"]....",
                    "CREATE (USER|LOGIN) .{0,200} (IDENTIFIED BY|WITH PASSWORD)", // sql creds
                    "(xox[pboa]-[0-9]{12}-[0-9]{12}-[0-9]{12}-[a-z0-9]{32})", //Slack Token
                    "https://hooks.slack.com/services/T[a-zA-Z0-9_]{8}/B[a-zA-Z0-9_]{8}/[a-zA-Z0-9_]{24}", //Slack Webhook
                    "aws[_\\-\\.]?key", // aws mnagic
                    "[_\\-\\.]?api[_\\-\\.]?key", // stuff
                    "[_\\-\\.]oauth\\s*=", // oauth stuff
                    "client_secret", // fun
                    "secret[_\\-\\.]?(key)?\\s*=",
                    "-----BEGIN( RSA| OPENSSH| DSA| EC| PGP)? PRIVATE KEY( BLOCK)?-----",
                    "(\\s|\\\'|\\\"|\\^|=)(A3T[A-Z0-9]|AKIA|AGPA|AROA|AIPA|ANPA|ANVA|ASIA)[A-Z0-9]{16}(\\s|\\\'|\\\"|$)", // aws access key
                }
            });
            // Firefox/Thunderbird backups
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be searched for Firefox/Thunderbird backups related strings.",
                RuleName = "browerContentByName",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileName,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepFFRegexRed",
                WordList = new List<string>()
                {
                    // Firefox/Thunderbird
                    "logins\\.json"
                },
            });
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with contents matching these regexes are very interesting.",
                RuleName = "KeepFFRegexRed",
                EnumerationScope = EnumerationScope.ContentsEnumeration,
                MatchLocation = MatchLoc.FileContentAsString,
                WordListType = MatchListType.Regex,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Red,
                WordList = new List<string>()
                {
                    "\"encryptedPassword\":\"[A-Za-z0-9+/=]+\""
                }
            });

            // vbscript etc
            
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Look inside unattend.xml files for actual values.",
                RuleName = "Unattend.xml",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileName,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepUnattendXmlRegexRed",
                WordList = new List<string>()
                {
                    "unattend\\.xml",
                    "Autounattend\\.xml"
                },
            });
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with contents matching these regexen are very interesting.",
                RuleName = "KeepUnattendXmlRegexRed",
                EnumerationScope = EnumerationScope.ContentsEnumeration,
                MatchLocation = MatchLoc.FileContentAsString,
                WordListType = MatchListType.Regex,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Red,
                WordList = new List<string>()
                {
                    "(?s)<AdministratorPassword>.{0,30}<Value>.*<\\/Value>",
                    "(?s)<AutoLogon>.{0,30}<Value>.*<\\/Value>"
                }
            });

            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Look inside unattend.xml files for actual values.",
                RuleName = "RDPFile",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepRdpPasswordRed",
                WordList = new List<string>()
                {
                    "\\.rdp"
                },
            });
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with contents matching these regexen are very interesting.",
                RuleName = "KeepRdpPasswordRed",
                EnumerationScope = EnumerationScope.ContentsEnumeration,
                MatchLocation = MatchLoc.FileContentAsString,
                WordListType = MatchListType.Regex,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Red,
                WordList = new List<string>()
                {
                    "password 51:b"
                }
            });

            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be subjected to a generic search for keys and such.",
                RuleName = "ConfigContentByExt",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepConfigRegexRed",
                WordList = new List<string>()
                {
                    "\\.yaml",
                    "\\.yml",
                    "\\.toml",
                    "\\.xml",
                    "\\.json",
                    "\\.config",
                    "\\.ini",
                    "\\.inf",
                    "\\.cnf",
                    "\\.conf",
                    "\\.properties",
                    "\\.env",
                    "\\.dist",
                    "\\.txt",
                    "\\.sql",
                    "\\.log",
                    "\\.sqlite",
                    "\\.sqlite3",
                    "\\.fdb"
                },
            });

            this.ClassifierRules.Add(new ClassifierRule()
            {
                RuleName = "KeepConfigRegexRed",
                EnumerationScope = EnumerationScope.ContentsEnumeration,
                MatchLocation = MatchLoc.FileContentAsString,
                WordListType = MatchListType.Regex,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Red,
                WordList = new List<string>()
                {
                    "sqlconnectionstring\\s*=\\s*[\\\'\\\"][^\\\'\\\"]....",
                    "connectionstring\\s*=\\s*[\\\'\\\"][^\\\'\\\"]....",
                    "validationkey\\s*=\\s*[\\\'\\\"][^\\\'\\\"]....",
                    "decryptionkey\\s*=\\s*[\\\'\\\"][^\\\'\\\"]....",
                    "passwo?r?d\\s*=\\s*[\\\'\\\"][^\\\'\\\"]....",
                    "CREATE (USER|LOGIN) .{0,200} (IDENTIFIED BY|WITH PASSWORD)", // sql creds
                    "(xox[pboa]-[0-9]{12}-[0-9]{12}-[0-9]{12}-[a-z0-9]{32})", //Slack Token
                    "https://hooks.slack.com/services/T[a-zA-Z0-9_]{8}/B[a-zA-Z0-9_]{8}/[a-zA-Z0-9_]{24}", //Slack Webhook
                    "aws[_\\-\\.]?key", // aws mnagic
                    "[_\\-\\.]?api[_\\-\\.]?key", // stuff
                    "[_\\-\\.]oauth\\s*=", // oauth stuff
                    "client_secret", // fun
                    "secret[_\\-\\.]?(key)?\\s*=",
                    "-----BEGIN( RSA| OPENSSH| DSA| EC| PGP)? PRIVATE KEY( BLOCK)?-----",
                    "(\\s|\\\'|\\\"|\\^|=)(A3T[A-Z0-9]|AKIA|AGPA|AROA|AIPA|ANPA|ANVA|ASIA)[A-Z0-9]{16}(\\s|\\\'|\\\"|$)", // aws access key
                    // network device config
                    "NVRAM config last updated",
                    "enable password \\.",
                    "simple-bind authenticated encrypt",
                },
            });
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files ending like this will be grepped for private keys.",
                RuleName = "CertContentByEnding",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileName,
                WordListType = MatchListType.EndsWith,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepCertRegexRed",
                WordList = new List<string>()
                {
                    "_rsa", // test file created
                    "_dsa", // test file created
                    "_ed25519", // test file created
                    "_ecdsa" // test file created
                },
            });

            this.ClassifierRules.Add(new ClassifierRule()
            {
                RuleName = "KeepCertRegexRed",
                EnumerationScope = EnumerationScope.ContentsEnumeration,
                MatchLocation = MatchLoc.FileContentAsString,
                WordListType = MatchListType.Regex,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Red,
                WordList = new List<string>()
                {
                    "-----BEGIN( RSA| OPENSSH| DSA| EC| PGP)? PRIVATE KEY( BLOCK)?-----"
                },
            });

            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files ending like this will be grepped for private keys.",
                RuleName = "CertContentByEndingYellow",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepCertRegexRed",
                WordList = new List<string>()
                {
                    "\\.pem",
                },
            });

            this.ClassifierRules.Add(new ClassifierRule()
            {
                RuleName = "KeepCertRegexYellow",
                EnumerationScope = EnumerationScope.ContentsEnumeration,
                MatchLocation = MatchLoc.FileContentAsString,
                WordListType = MatchListType.Regex,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Yellow,
                WordList = new List<string>()
                {
                    "-----BEGIN( RSA| OPENSSH| DSA| EC| PGP)? PRIVATE KEY( BLOCK)?-----"
                },
            });

            this.ClassifierRules.Add(
                new ClassifierRule()
                {
                    Description = "Files with these extensions will be parsed as x509 certificates to see if they have private keys.",
                    RuleName = "KeepCertContainsPrivKeyRed",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileExtension,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.CheckForKeys,
                    Triage = Triage.Red,
                    WordList = new List<string>()
                    {
                        "\\.der",   // test file created
                        "\\.pfx",
                        "\\.pk12",
                        "\\.p12",
                        "\\.pkcs12",
                    },
                }
            );
        }
    }
}
