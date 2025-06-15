using System.Collections.Generic;
using SnaffCore.Classifiers;
using CsToml;

namespace Snaffler
{
    [TomlSerializedObject]
    public partial class RuleSet
    {
        [TomlValueOnSerialized]
        public List<ClassifierRule> ClassifierRules { get; set; } = new List<ClassifierRule>();
    }
}