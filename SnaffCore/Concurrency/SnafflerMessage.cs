using System;
using SnaffCore.ShareFind;
using SnaffCore.ShareScan;

namespace SnaffCore.Concurrency
{
    public class SnafflerMessage
    {
        public DateTime DateTime { get; set; }
        public string Message { get; set; }
        public SnafflerMessageType Type { get; set; }
        public FileScanner.FileResult FileResult { get; set; }
        public ShareFinder.ShareResult ShareResult { get; set; }
        public TreeWalker.DirResult DirResult { get; set; }
    }
}