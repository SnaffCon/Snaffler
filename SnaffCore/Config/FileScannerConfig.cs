namespace SnaffCore.Config
{
    public partial class Config
    {
        // whether or not to do file scans at all
        public string DirTarget { get; set; }

        // these bools enable/disable the various methods for checking if we want to keep/ignore a file.
        public bool ExactExtensionCheck { get; set; } = true;
        public bool ExactExtensionSkipCheck { get; set; } = true;
        public bool PartialPathCheck { get; set; } = true;
        public bool ExactNameCheck { get; set; } = true;
        public bool PartialNameCheck { get; set; } = true;
        public bool GrepByExtensionCheck { get; set; } = true;

        public long MaxSizeToGrep { get; set; } = 500000;

        // these enable or disable automated downloading of files that match the criteria
        public bool EnableMirror { get; set; } = false;
        public string MirrorPath { get; set; }
        public int GrepContextBytes { get; set; } = 0;
    }
}