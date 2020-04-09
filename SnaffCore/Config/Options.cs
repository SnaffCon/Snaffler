using System.Linq;
using NLog;
using Classifiers;

namespace SnaffCore.Config
{
    public partial class Options
    {
        // Manual Targeting Options

        public string[] PathTargets { get; set; }
        public string[] ComputerTargets { get; set; }

        // Concurrency Options
        public int MaxThreads { get; set; } = 30;

        // Logging Options
        public bool LogToFile { get; set; } = false;
        public string LogFilePath { get; set; }
        public bool LogToConsole { get; set; } = true;
        public LogLevel LogLevel { get; set; } = LogLevel.Info;

        // ShareFinder Options
        public bool ShareFinderEnabled { get; set; } = true;
        public string TargetDomain { get; set; }
        public string TargetDc { get; set; }

        // ShareScanner Options
        public bool ShareScanEnabled { get; set; } = true;

        // FileScanner Options

        // this sets the maximum size of file to look inside.
        public long MaxSizeToGrep { get; set; } = 500000;

        // these enable or disable automated downloading of files that match the criteria
        public bool EnableMirror { get; set; } = false;
        public string MirrorPath { get; set; }
        public int GrepContextBytes { get; set; } = 0;

        public Options(bool defaults = true)
        {
            if (defaults)
            {
                BuildDefaultClassifiers();
                ShareClassifiers = (from classifier in Classifiers
                    where classifier.EnumerationScope == EnumerationScope.ShareEnumeration
                    select classifier).ToList();
                DirClassifiers = (from classifier in Classifiers
                    where classifier.EnumerationScope == EnumerationScope.DirectoryEnumeration
                    select classifier).ToList();
                FileClassifiers = (from classifier in Classifiers
                    where classifier.EnumerationScope == EnumerationScope.FileEnumeration
                    select classifier).ToList();
                ContentsClassifiers = (from classifier in Classifiers
                    where classifier.EnumerationScope == EnumerationScope.ContentsEnumeration
                    select classifier).ToList();
            }
        }
    }
}