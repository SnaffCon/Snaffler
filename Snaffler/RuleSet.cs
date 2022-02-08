using System.Collections.Generic;
using SnaffCore.Classifiers;

namespace Snaffler
{
    public class RuleSet
    {
        public List<ClassifierRule> ClassifierRules { get; set; } = new List<ClassifierRule>();
    }
}