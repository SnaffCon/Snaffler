using System.Collections.Generic;
using Classifiers;

namespace SnaffCore.Config
{
    public partial class Options
    {
        private void BuildShareClassifiers()
        {
            this.ClassifierRules.Add(new ClassifierRule()
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
            });
            this.ClassifierRules.Add(new ClassifierRule()
            {
                RuleOrder = 1,
                RuleName = "KeepCDollaShare",
                EnumerationScope = EnumerationScope.ShareEnumeration,
                MatchLocation = MatchLoc.ShareName,
                MatchAction = MatchAction.Snaffle,
                WordListType = MatchListType.EndsWith,
                WordList = new List<string>()
                    {
                        "\\C$"
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
