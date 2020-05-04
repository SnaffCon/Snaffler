using System.Collections.Generic;
using Classifiers;

namespace SnaffCore.Config
{
    public partial class Options
    {
        private void BuildSimpleGrepClassifiers()
        {
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
