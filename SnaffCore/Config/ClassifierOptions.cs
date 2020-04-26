using System.Collections.Generic;
using System.Linq;
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