using System;
using SnaffCore.ShareFind;
using SnaffCore.ShareScan;

namespace SnaffCore.Config
{
    public enum SnafflerMessageType
    {
        Error,
        ShareResult,
        FileResult,
        Info,
        Degub,
        Trace,
        Fatal
    }

    public class SnafflerMessage
    {
        public DateTime DateTime { get; set; }
        public string Message { get; set; }
        public SnafflerMessageType Type { get; set; }
        public FileScanner.FileResult FileResult { get; set; }
        public ShareFinder.ShareResult ShareResult { get; set; }
    }
}