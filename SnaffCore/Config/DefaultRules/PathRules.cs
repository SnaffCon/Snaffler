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
                        @"servicing\\packages",
                        @"Microsoft.NET\\Framework",
                        @"windows\\immersivecontrolpanel",
                        @"windows\\diagnostics",
                        @"windows\\debug",
                        "node_modules",
                        @"vendor\\bundle",
                        @"vendor\\cache",
                        @"locale\\",
                        @"chocolatey\\helpers",
                        @"sources\\sxs",
                        @"localization\\",
                        @"\\AppData\\Local\\Microsoft\\",
                        @"\\AppData\\Roaming\\Microsoft\\",
                        @"\\wsuscontent",
                        @"\\Application Data\\Microsoft\\CLR Security Config\\",
                        @"\\doc\\openssl",
                        @"\\puppet\\share\\doc",
                        @"\\lib\\ruby\\",
                        @"\\lib\\site-packages",
                        @"\\usr\\share\\doc",
                        @"\\servicing\\LCU\\"
                    },
            });

            this.ClassifierRules.Add(
                new ClassifierRule()
                {
                    Description = "Files with a path containing these strings are very interesting.",
                    RuleName = "KeepPathContainsRed",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FilePath,
                    WordListType = MatchListType.Contains,
                    MatchAction = MatchAction.Snaffle,
                    Triage = Triage.Red,
                    WordList = new List<string>()
                {
                        "\\\\.ssh\\\\", // test file created
                        "\\\\.purple\\\\accounts.xml", // test file created
                        "\\\\.aws\\\\", // test file created
                        "\\\\.gem\\\\credentials", // test file created
                        "doctl\\\\config.yaml", // test file created
                        "config\\\\hub",  // test file created
                        "control\\\\customsettings.ini"
                },
                }
            );

        }
    }
}
