using Classifiers;
using System.Collections.Generic;

namespace SnaffCore.Config
{
    public partial class Options
    {
        private void BuildFileDiscardRules()
        {
            this.ClassifierRules.Add(
                new ClassifierRule()
                {
                    Description = "Skip any further scanning for files with these extensions.",
                    RuleName = "DiscardExtExact",
                    EnumerationScope = EnumerationScope.FileEnumeration,
                    MatchLocation = MatchLoc.FileExtension,
                    WordListType = MatchListType.Exact,
                    MatchAction = MatchAction.Discard,
                    WordList = new List<string>()
                    {
                        // always skip these file extensions
                        // image formats
                        ".bmp",
                        ".eps",
                        ".gif",
                        ".ico",
                        ".jfi",
                        ".jfif",
                        ".jif",
                        ".jpe",
                        ".jpeg",
                        ".jpg",
                        ".png",
                        ".psd",
                        ".svg",
                        ".tif",
                        ".tiff",
                        ".webp",
                        ".xcf",
                        // fonts
                        ".ttf",
                        ".otf",
                        // misc
                        ".lock",
                        ".css",
                        ".less"
                    },
                });
        }
    }
}
