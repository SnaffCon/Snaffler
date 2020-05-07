using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Classifiers;

namespace SnaffCore.Config
{
    public partial class Options
    {
        // Classifiers
        public List<ClassifierRule> ClassifierRules { get; set; } = new List<ClassifierRule>();
        [Nett.TomlIgnore]
        public List<ClassifierRule> ShareClassifiers { get; set; } = new List<ClassifierRule>();
        [Nett.TomlIgnore]
        public List<ClassifierRule> DirClassifiers { get; set; } = new List<ClassifierRule>();
        [Nett.TomlIgnore]
        public List<ClassifierRule> FileClassifiers { get; set; } = new List<ClassifierRule>();
        [Nett.TomlIgnore]
        public List<ClassifierRule> ContentsClassifiers { get; set; } = new List<ClassifierRule>();

        public void PrepareClassifiers()
        {

            if (this.ClassifierRules.Count <= 0)
            {
                this.BuildDefaultClassifiers();
            }

            // Where rules are using regexen, we precompile them here.
            // We're gonna use them a lot so efficiency matters.
            foreach (ClassifierRule classifierRule in ClassifierRules)
            {
                if (classifierRule.WordListType == MatchListType.Regex)
                {
                    classifierRule.Regexes = new List<Regex>();
                    foreach (string pattern in classifierRule.WordList)
                    {
                        classifierRule.Regexes.Add(new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant));
                    }
                }
                else
                {
                    switch (classifierRule.WordListType)
                    {
                        case MatchListType.Contains:
                            classifierRule.Regexes = new List<Regex>();
                            foreach (string word in classifierRule.WordList)
                            {
                                string pattern = Regex.Escape(word);
                                classifierRule.Regexes.Add(new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
                            }
                            break;
                        case MatchListType.EndsWith:
                            foreach (string word in classifierRule.WordList)
                            {
                                string pattern = Regex.Escape(word);
                                pattern = pattern + "$";
                                classifierRule.Regexes.Add(new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
                            }
                            break;
                        case MatchListType.StartsWith:
                            foreach (string word in classifierRule.WordList)
                            {
                                string pattern = Regex.Escape(word);
                                pattern = "^" + pattern;
                                classifierRule.Regexes.Add(new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
                            }
                            break;
                        case MatchListType.Exact:
                            foreach (string word in classifierRule.WordList)
                            {
                                string pattern = Regex.Escape(word);
                                pattern = "^" + pattern + "$";
                                classifierRule.Regexes.Add(new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
                            }
                            break;
                    }
                }
            }

            ShareClassifiers = (from classifier in ClassifierRules
                where classifier.EnumerationScope == EnumerationScope.ShareEnumeration
                select classifier).ToList();
            DirClassifiers = (from classifier in ClassifierRules
                where classifier.EnumerationScope == EnumerationScope.DirectoryEnumeration
                select classifier).ToList();
            FileClassifiers = (from classifier in ClassifierRules
                where classifier.EnumerationScope == EnumerationScope.FileEnumeration
                select classifier).ToList();
            ContentsClassifiers = (from classifier in ClassifierRules
                where classifier.EnumerationScope == EnumerationScope.ContentsEnumeration
                select classifier).ToList();
        }

        private void BuildDefaultClassifiers()
        {
            this.ClassifierRules = new List<ClassifierRule>();
            
            BuildShareClassifiers();
            BuildPathClassifiers();
            BuildFileDiscardClassifiers();
            BuildFileKeepClassifiers();
            BuildSimpleGrepClassifiers();
            BuildFileRegexClassifiers();
        }
    }
}