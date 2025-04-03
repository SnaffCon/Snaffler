using System.ComponentModel;

namespace SnaffCore.Concurrency
{
    public enum SnafflerMessageType
    {
        Error,
        [Description("Share")]
        ShareResult,
        [Description("Dir")]
        DirResult,
        [Description("File")]
        FileResult,
        Finish,
        Info,
        Degub,
        Trace,
        Fatal
    }
}