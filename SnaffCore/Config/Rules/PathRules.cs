using System.Collections.Generic;
using Classifiers;

namespace SnaffCore.Config
{
    public partial class Options
    {
        private void BuildPathClassifiers()
        {
            this.ClassifierRules.Add(new ClassifierRule()
            {
                RuleOrder = 0,
                RuleName = "DiscardFilepathContains",
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
            });
        }
    }
}
