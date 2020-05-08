using System.Collections.Generic;
using Classifiers;

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
                        "\\print$",
                        "\\ipc$"
                    },
            });
            this.ClassifierRules.Add(new ClassifierRule()
            {
                RuleName = "KeepCDollaShare",
                Description = "Notifies the user that they can read C$ or ADMIN$ but doesn't actually scan inside it.",
                EnumerationScope = EnumerationScope.ShareEnumeration,
                MatchLocation = MatchLoc.ShareName,
                MatchAction = MatchAction.Snaffle,
                WordListType = MatchListType.EndsWith,
                WordList = new List<string>()
                    {
                        "\\C$",
                        "\\ADMIN$"
                    },
            });
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
