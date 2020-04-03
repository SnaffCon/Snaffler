namespace SnaffCore.Config
{
    public partial class Options
    {
        public bool ShareFinderEnabled { get; set; } = true;
        public string TargetDomain { get; set; }
        public string TargetDc { get; set; }
    }
}