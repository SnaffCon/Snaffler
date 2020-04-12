using System.Linq;
using NLog;
using Classifiers;
using SnaffCore.Concurrency;

namespace SnaffCore.Config
{
    public partial class Options
    {
        // Manual Targeting Options
        public string[] PathTargets { get; set; }
        public string[] ComputerTargets { get; set; }
        public bool ScanSysvol { get; set; } = true;
        public bool ScanNetlogon { get; set; } = true;

        // Concurrency Options
        public int MaxThreads { get; set; } = 30;

        // Logging Options
        public bool LogToFile { get; set; } = false;
        public string LogFilePath { get; set; }
        public bool LogToConsole { get; set; } = true;

        [Nett.TomlIgnore]
        public LogLevel LogLevel { get; set; } = LogLevel.Info;
        public string LogLevelString { get; set; } = "info";

        // ShareFinder Options
        public bool ShareFinderEnabled { get; set; } = true;
        public string TargetDomain { get; set; }
        public string TargetDc { get; set; }

        // FileScanner Options

        // this sets the maximum size of file to look inside.
        public long MaxSizeToGrep { get; set; } = 1000000;

        // these enable or disable automated downloading of files that match the criteria
        public bool Snaffle { get; set; } = false;
        public long MaxSizeToSnaffle { get; set; } = 10000000;
        public string SnafflePath { get; set; }

        // Content processing options
        public int GrepContextBytes { get; set; } = 0;

        public Options()
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

        public void ParseLogLevelString(string logLevelString)
        {
            BlockingMq Mq = BlockingMq.GetMq();
            switch (logLevelString.ToLower())
            {
                case "debug":
                    LogLevel = LogLevel.Debug;
                    Mq.Degub("Set verbosity level to degub.");
                    break;
                case "degub":
                    LogLevel = LogLevel.Debug;
                    Mq.Degub("Set verbosity level to degub.");
                    break;
                case "trace":
                    LogLevel = LogLevel.Trace;
                    Mq.Degub("Set verbosity level to trace.");
                    break;
                case "data":
                    LogLevel = LogLevel.Warn;
                    Mq.Degub("Set verbosity level to data.");
                    break;
                case "info":
                    LogLevel = LogLevel.Info;
                    Mq.Degub("Set verbosity level to info.");
                    break;
                default:
                    LogLevel = LogLevel.Info;
                    Mq.Error("Invalid verbosity level " + logLevelString +
                             " falling back to default level (info).");
                    break;
            }
        }
    }
}