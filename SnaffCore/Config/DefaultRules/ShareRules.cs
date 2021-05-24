using Classifiers;
using System.Collections.Generic;

namespace SnaffCore.Config
{
    public partial class Options
    {
        private void BuildShareRules()
        {
            this.ClassifierRules.Add(new ClassifierRule()
            {
                RuleName = "DiscardShareEndsWith",
                Description = "Skips scanning inside shares ending with these words.",
                EnumerationScope = EnumerationScope.ShareEnumeration,
                MatchLocation = MatchLoc.ShareName,
                MatchAction = MatchAction.Discard,
                WordListType = MatchListType.EndsWith,
                WordList = new List<string>()
                    {
                        @"\\{2}print\$",
                        @"\\{2}ipc\$"
                    },
            });
            this.ClassifierRules.Add(new ClassifierRule()
            {
                RuleName = "KeepShareBlack",
                Description = "Notifies the user that they can read C$ or ADMIN$ or something fun/noisy, but doesn't actually scan inside it.",
                EnumerationScope = EnumerationScope.ShareEnumeration,
                MatchLocation = MatchLoc.ShareName,
                MatchAction = MatchAction.Snaffle,
                WordListType = MatchListType.EndsWith,
                Triage = Triage.Black,
                WordList = new List<string>()
                    {
                        @"\\{2}C\$",
                        @"\\{2}ADMIN\$"
                    },
            });
            /*
            this.ClassifierRules.Add(new ClassifierRule()
            {
                RuleName = "KeepShareRed",
                Description = "Notifies the user that they can read C$ or ADMIN$ or something fun/noisy, but doesn't actually scan inside it.",
                EnumerationScope = EnumerationScope.ShareEnumeration,
                MatchLocation = MatchLoc.ShareName,
                MatchAction = MatchAction.Snaffle,
                WordListType = MatchListType.EndsWith,
                Triage = Triage.Black,
                WordList = new List<string>()
                    {
                        "\\Users",
                    },
            });
            */
        }

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
