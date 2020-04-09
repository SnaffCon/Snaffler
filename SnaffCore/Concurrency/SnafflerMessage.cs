using System;
using Classifiers;

namespace SnaffCore.Concurrency
{
    public class SnafflerMessage
    {
        public DateTime DateTime { get; set; }
        public string Message { get; set; }
        public SnafflerMessageType Type { get; set; }
        public FileResult FileResult { get; set; }
        public ShareResult ShareResult { get; set; }
        public DirResult DirResult { get; set; }
    }
}