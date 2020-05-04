using System.Collections.Generic;
using Classifiers;

namespace SnaffCore.Config
{
    public partial class Options
    {
        private void BuildFileRegexClassifiers()
        {
            // Python
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be searched for python related strings.",
                RuleName = "PyGrepExtExact",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepPyGrepContainsRed",
                WordList = new List<string>()
                {
                    // python
                    ".py"
                },
            });

            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with contents matching these regexen are very interesting.",
                RuleName = "KeepPyGrepContainsRed",
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
                }
            });
            // PHP
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be searched for php related strings.",
                RuleName = "phpGrepExtExact",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepPhpGrepContainsRed",
                WordList = new List<string>()
                {
                    // php
                    ".php",
                    ".phtml",
                    ".inc",
                    ".php3",
                    ".php5",
                    ".php7"
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
                    // php
                    "mysql_connect[[:space:]]*\\(.*\\$.*\\)", // php
                    "mysql_pconnect[[:space:]]*\\(.*\\$.*\\)", // php
                    "mysql_change_user[[:space:]]*\\(.*\\$.*\\)", // php
                    "pg_connect[[:space:]]*\\(.*\\$.*\\)", // php
                    "pg_pconnect[[:space:]]*\\(.*\\$.*\\)", // php
                }
            });
            // CSharp
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be searched for CSharp and ASP.NET related strings.",
                RuleName = "csGrepExtExact",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepCsGrepContainsRed",
                WordList = new List<string>()
                {
                    // asp.net
                    ".aspx",
                    ".ashx",
                    ".asmx",
                    ".asp",
                    ".cshtml",
                    ".cs",
                    ".ascx"
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
                    // csharp
                    "new OleDbConnection\\(", // asp.net
                }
            });
            //Java
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be searched for Java and ColdFusion related strings.",
                RuleName = "javaGrepExtExact",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepJavaGrepContainsRed",
                WordList = new List<string>()
                {
                    // java
                    ".jsp",
                    ".do",
                    ".java",
                    // coldfusion
                    ".cfm",
                },
            });
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with contents matching these regexen are very interesting.",
                RuleName = "KeepJavaGrepContainsRed",
                EnumerationScope = EnumerationScope.ContentsEnumeration,
                MatchLocation = MatchLoc.FileContentAsString,
                WordListType = MatchListType.Regex,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Red,
                WordList = new List<string>()
                {
                    // java
                    "\\.getConnection\\(\\\"jdbc\\:",
                }
            });
            // Ruby
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be searched for Rubby related strings.",
                RuleName = "rubyGrepExtExact",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepRubyGrepContainsRed",
                WordList = new List<string>()
                {
                    // ruby
                    ".rb"
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
                }
            });

            // Perl
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be searched for Perl related strings.",
                RuleName = "perlGrepExtExact",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepPerlGrepContainsRed",
                WordList = new List<string>()
                {
                    // perl
                    ".pl"
                },
            });
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with contents matching these regexen are very interesting.",
                RuleName = "KeepPerlGrepContainsRed",
                EnumerationScope = EnumerationScope.ContentsEnumeration,
                MatchLocation = MatchLoc.FileContentAsString,
                WordListType = MatchListType.Regex,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Red,
                WordList = new List<string>()
                {
                    // perl
                    "DBI\\-\\>connect\\(",
                }
            });

            // PowerShell
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be searched for PowerShell related strings.",
                RuleName = "psGrepExtExact",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepPsGrepContainsRed",
                WordList = new List<string>()
                {
                    // powershell
                    ".psd1",
                    ".psm1",
                    ".ps1",
                },
            });
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with contents matching these regexen are very interesting.",
                RuleName = "KeepPsGrepContainsRed",
                EnumerationScope = EnumerationScope.ContentsEnumeration,
                MatchLocation = MatchLoc.FileContentAsString,
                WordListType = MatchListType.Regex,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Red,
                WordList = new List<string>()
                {
                    // PS
                    " -Credential ",
                    "net user ",
                    "psexec .{0-100} -p ",
                    "-SecureString"
                }
            });

            // Batch
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be searched for cmd.exe/batch file related strings.",
                RuleName = "cmdGrepExtExact",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepCmdGrepContainsRed",
                WordList = new List<string>()
                {
                    // cmd.exe
                    ".bat"
                },
            });
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with contents matching these regexen are very interesting.",
                RuleName = "KeepCmdGrepContainsRed",
                EnumerationScope = EnumerationScope.ContentsEnumeration,
                MatchLocation = MatchLoc.FileContentAsString,
                WordListType = MatchListType.Regex,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Red,
                WordList = new List<string>()
                {
                    // cmd
                    "net user ",
                    "psexec .{0-100} -p "
                }
            });

            // bash/sh/zsh/etc
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be searched for Bash related strings.",
                RuleName = "bashGrepExtExact",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepBashGrepContainsRed",
                WordList = new List<string>()
                {
                    // powershell
                    ".sh",
                    ".rc",
                    ".profile"
                },
            });
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with contents matching these regexen are very interesting.",
                RuleName = "KeepBashGrepContainsRed",
                EnumerationScope = EnumerationScope.ContentsEnumeration,
                MatchLocation = MatchLoc.FileContentAsString,
                WordListType = MatchListType.Regex,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Red,
                WordList = new List<string>()
                {
                    // bash
                    "sshpass -p.*['|\\\"]", //SSH Password
                }
            });

            // vbscript etc
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be searched for VBScript related strings.",
                RuleName = "vbsGrepExtExact",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepVbsGrepContainsRed",
                WordList = new List<string>()
                {
                    ".vbs",
                    ".wsf"
                },
            });
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with contents matching these regexen are very interesting.",
                RuleName = "KeepVbsGrepContainsRed",
                EnumerationScope = EnumerationScope.ContentsEnumeration,
                MatchLocation = MatchLoc.FileContentAsString,
                WordListType = MatchListType.Regex,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Red,
                WordList = new List<string>()
                    { }
            });

            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be subjected to a generic search for keys and such.",
                RuleName = "ConfigGrepExtExact",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepConfigGrepContainsRed",
                WordList = new List<string>()
                {
                    ".yaml",
                    ".yml",
                    ".toml",
                    ".xml",
                    ".json",
                    ".config",
                    ".ini",
                    ".inf",
                    ".cnf",
                    ".conf",
                    ".properties",
                    ".env",
                    ".dist",
                    ".txt",
                    ".sql",
                    ".log",
                    ".sqlite",
                    ".sqlite3",
                    ".fdb"
                },
            });

            this.ClassifierRules.Add(new ClassifierRule()
            {
                RuleName = "KeepConfigGrepContainsRed",
                EnumerationScope = EnumerationScope.ContentsEnumeration,
                MatchLocation = MatchLoc.FileContentAsString,
                WordListType = MatchListType.Regex,
                MatchAction = MatchAction.Snaffle,
                Triage = Triage.Red,
                WordList = new List<string>()
                {
                    "sqlConnectionString\\( |\\)=\\( |\\)\"..*?\"",
                    "connectionString\\( |\\)=\\( |\\)\"..*?\"",
                    "validationKey\\( |\\)=\\( |\\)\"..*?\"",
                    "decryptionKey\\( |\\)=\\( |\\)\"..*?\"",
                    "[Pp][Aa][Ss][Ss][Ww][Oo]?[Rr]?[Dd][[:space:]]*=[[:space:]]*[\\\'\\\"][^\\\'\\\"].....*",
                    "CREATE (USER|LOGIN) .{0,100} (IDENTIFIED BY|WITH PASSWORD)", // sql creds
                    "(xox[pboa]-[0-9]{12}-[0-9]{12}-[0-9]{12}-[a-z0-9]{32})", //Slack Token
                    "https://hooks.slack.com/services/T[a-zA-Z0-9_]{8}/B[a-zA-Z0-9_]{8}/[a-zA-Z0-9_]{24}", //Slack Webhook
                    "[Aa][Ww][Ss][_\\-\\.]?[Ss][Ee][Cc][Rr][Ee][Tt][_\\-\\.]?[Kk][Ee][Yy]", // aws magic
                    "[Aa][Ww][Ss][_\\-\\.]?[Kk][Ee][Yy]", // aws mnagic
                    "[_\\-\\.]?[Aa][Pp][Ii][_\\-\\.]?[Kk][Ee][Yy]", // stuff
                    "[_\\-\\.][Oo][Aa][Uu][Tt][Hh][[:space:]]*=", // oauth stuff
                    "(client_secret|CLIENT_SECRET)", // fun
                    "[Ss][Ee][Cc][Rr][Ee][Tt][_\\-\\.]?([Kk][Ee][Yy])?[[:space:]]*=",
                    "-----BEGIN( RSA| OPENSSH| DSA| EC| PGP)? PRIVATE KEY( BLOCK)?-----",
                    "(A3T[A-Z0-9]|AKIA|AGPA|AROA|AIPA|ANPA|ANVA|ASIA)[A-Z0-9]{16}", // aws access key
                    // network device config
                    "NVRAM config last updated",
                    "enable password .",
                },
            });
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be grepped for private keys.",
                RuleName = "CertGrepExtExact",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileExtension,
                WordListType = MatchListType.Exact,
                MatchAction = MatchAction.Relay,
                RelayTarget = "KeepCertGrepContainsRed",
                WordList = new List<string>()
                {
                    "_rsa",
                    "_dsa",
                    "_ed25519",
                    "_ecdsa",
                    "_ed25519",
                    ".pem",
                },
            });

            this.ClassifierRules.Add(new ClassifierRule()
            {
                RuleName = "KeepCertGrepContainsRed",
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
        }
    }
}