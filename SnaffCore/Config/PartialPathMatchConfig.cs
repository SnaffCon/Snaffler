namespace SnaffCore.Config
{
    public partial class Options
    {
        public string[] PathsToKeep { get; set; } =
        {
            ".ssh\\config",
            ".purple\\accounts.xml",
            ".aws\\credentials",
            ".gem\\credentials",
            "doctl\\config.yaml",
            "config\\hub"
        };
    }
}