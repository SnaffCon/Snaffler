namespace SnaffCore.Config
{
    public partial class Options
    {
        public bool ShareScanEnabled { get; set; } = true;
        public bool ScanCDollarShares { get; set; } = false;

        public string[] ShareSkipList { get; set; } =
        {
            // these are share names that make us skip the share instantly.
            "print$"
        };

        public string[] ShareStringsToPrioritise { get; set; } =
        {
            // these are substrings that make a share or hostname more interesting and make it worth prioritising.
            "IT",
            "security",
            "admin",
            "dev",
            "sql",
            "backup",
            "sap",
            "erp",
            "oracle",
            "vmware",
            "sccm"
        };
    }
}