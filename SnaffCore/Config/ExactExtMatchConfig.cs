namespace SnaffCore.Config
{
    public partial class Config
    {
        public string[] ExtensionsToKeep { get; set; } =
        {
            // these are file extensions that we will always want to keep no matter what.
            ".kdbx",
            ".kdb",
            ".ppk",
            ".vmdk",
            ".vhd",
            ".rdp",
            ".der",
            ".key",
            ".pk12",
            ".bak",
            ".p12",
            ".pkcs12",
            ".pfx",
            ".asc",
            ".ovpn",
            ".cscfg",
            ".mdf",
            ".sdf",
            ".sqlite",
            ".sqlite3",
            ".bek",
            ".tpm",
            ".fve",
            ".psafe3",
            ".jks",
            ".pcap",
            ".kwallet",
            ".tblk",
            ".key",
            ".keypair",
            ".sqldump",
            ".ova",
            ".ovf",
            ".wim"
        };
    }
}