using NLog;

namespace SnaffCore.Config
{
    public partial class Config
    {
        public bool LogToFile { get; set; } = false;
        public string LogFilePath { get; set; }
        public bool LogToConsole { get; set; } = true;
        public LogLevel LogLevel { get; set; } = LogLevel.Info;
        public bool DegubLog { get; set; } = false;
        public bool VerboseLog { get; set; } = false;
    }
}