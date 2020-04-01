namespace SnaffCore.Config
{
    public partial class Config
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