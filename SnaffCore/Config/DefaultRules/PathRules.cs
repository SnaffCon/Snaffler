using Classifiers;
using System.Collections.Generic;

namespace SnaffCore.Config
{
    public partial class Options
    {
        private void BuildPathRules()
        {
            this.ClassifierRules.Add(new ClassifierRule()
            {
                Description = "File paths that will be skipped entirely.",
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
                        "windows\\immersivecontrolpanel",
                        "windows\\diagnostics",
                        "windows\\debug",
                        "node_modules",
                        "vendor\\bundle",
                        "vendor\\cache",
                        "locale\\",
                        "chocolatey\\helpers",
                        "sources\\sxs",
                        "localization\\",
                        "\\AppData\\Local\\Microsoft\\",
                        "\\AppData\\Roaming\\Microsoft\\",
                        "\\wsuscontent",
                        "\\Application Data\\Microsoft\\CLR Security Config\\"
                    },
            });
        }
    }
}
