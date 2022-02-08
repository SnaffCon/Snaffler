using Classifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
            // Where rules are using regexen, we precompile them here.
            // We're gonna use them a lot so efficiency matters.
            foreach (ClassifierRule classifierRule in ClassifierRules)
            {
                classifierRule.Regexes = new List<Regex>();
                switch (classifierRule.WordListType)
                {
                    case MatchListType.Regex:
                        foreach (string pattern in classifierRule.WordList)
                        {
                            classifierRule.Regexes.Add(new Regex(pattern,
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
                        }

                        break;
                    case MatchListType.Contains:
                        classifierRule.Regexes = new List<Regex>();
                        foreach (string pattern in classifierRule.WordList)
                        {
                            classifierRule.Regexes.Add(new Regex(pattern,
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
                        }

                        break;
                    case MatchListType.EndsWith:
                        foreach (string pattern in classifierRule.WordList)
                        {
                            classifierRule.Regexes.Add(new Regex(pattern + "$",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
                        }

                        break;
                    case MatchListType.StartsWith:
                        foreach (string pattern in classifierRule.WordList)
                        {
                            classifierRule.Regexes.Add(new Regex("^" + pattern,
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
                        }

                        break;
                    case MatchListType.Exact:
                        foreach (string pattern in classifierRule.WordList)
                        {
                            classifierRule.Regexes.Add(new Regex("^" + pattern + "$",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
                        }

                        break;

                }
            }

            // figure out which rules match our interest level flag
            ClassifierRules = (from classifier in ClassifierRules
                               where IsInterest(classifier)
                               select classifier).ToList();
            // sort everything into enumeration scopes
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

        private bool IsInterest(ClassifierRule classifier)
        {
            /*
             * Keep all discard & archive parsing rules.
             * Else, if rule (or child rule, recursive) interest level is lower than provided (0 default), then discard
             */
            if (classifier.RelayTargets != null)
            {
                int max = 0;
                foreach (string relayTarget in classifier.RelayTargets)
                {
                    ClassifierRule relayRule = ClassifierRules.First(thing => thing.RuleName == relayTarget);
                    if (
                        (relayRule.Triage == Triage.Black && InterestLevel >3) ||
                        (relayRule.Triage == Triage.Red && InterestLevel > 2) ||
                    (relayRule.Triage == Triage.Yellow && InterestLevel > 1) ||
                    (relayRule.Triage == Triage.Green && InterestLevel > 0))
                    {
                        return true;
                    }
                }
            }
            return !(
                (
                    classifier.MatchAction == MatchAction.Snaffle ||
                    classifier.MatchAction == MatchAction.CheckForKeys
                ) &&
                (
                (classifier.Triage == Triage.Black && InterestLevel > 3) ||
                    (classifier.Triage == Triage.Red && InterestLevel > 2) ||
                    (classifier.Triage == Triage.Yellow && InterestLevel > 1) ||
                    (classifier.Triage == Triage.Green && InterestLevel > 0)
                )
            );
        }
        public void BuildDefaultClassifiers()
        {
            this.ClassifierRules = new List<ClassifierRule>();
            BuildShareRules();
            BuildPathRules();
            BuildFileDiscardRules();
            BuildFileNameRules();
            BuildFileContentRules();
        }
    }
}