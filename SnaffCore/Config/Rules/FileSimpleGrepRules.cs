using System.Collections.Generic;
using Classifiers;

namespace SnaffCore.Config
{
    public partial class Options
    {
        private void BuildSimpleGrepClassifiers()
        {
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be subjected to a generic search for keys and such.",
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
                        ".fdb"
                    },
            });

            this.ClassifierRules.Add(new ClassifierRule()
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
                        ".xml",
                        ".json",
                        ".config",
                        ".ini",
                        ".inf",
                        ".cnf",
                        ".conf",
                    },
            });

            this.ClassifierRules.Add(new ClassifierRule()
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
            });

            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "Files with these extensions will be grepped for private keys.",
                RuleName = "PrivKeyGrepNameEndsWith",
                EnumerationScope = EnumerationScope.FileEnumeration,
                MatchLocation = MatchLoc.FileName,
                WordListType = MatchListType.EndsWith,
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
            });

            this.ClassifierRules.Add(new ClassifierRule()
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
            });

            /*
            this.ClassifierRules.Add(new ClassifierRule()
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
            });
            */
        }
    }
}
