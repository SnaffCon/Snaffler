namespace SnaffCore.Config
{
    public partial class Options
    {
        [Nett.TomlIgnore]
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