﻿namespace SnaffCore.Config
{
    public partial class Config
    {
        public string[] NameStringsToKeep { get; set; } =
        {
            // these are strings that make a file NAME interesting if found within.
            "passw",
            "as-built",
            "handover",
            "secret",
            "thycotic",
            "cyberark",
            "_rsa",
            "_dsa",
            "_ed25519",
            "_ecdsa"
        };
    }
}