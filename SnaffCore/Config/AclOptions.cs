using System.Collections.Generic;

namespace SnaffCore.Config
{
    public class TrusteeOption
    {
        public string SID { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool DomainSID { get; set; } // is this the sid of a domain group/account?
        public bool LocalSID { get; set; } // is this the sid of a local group/account?
        public bool HighPriv { get; set; } // is this group/account canonically high-priv, i.e. do they have well-known paths to get local Admin/Domain Admin by default?
        public bool LowPriv { get; set; } // is this group/account canonically low-priv, i.e. are they one of the massive default groups like Domain Users etc.
        public bool Target { get; set; }
    }

    public partial class AclOptions
    {
        public AclOptions()
        {
            LoadTrusteeOptions();
        }
        public List<TrusteeOption> TrusteeOptions { get; set; }
        public bool SuckItAndSee { get; set; } = false;
        public List<TrusteeOption> TargetTrustees { get; set; }

        public List<string> InterestingRights { get; set; } = new List<string>()
        {
            "Owner",
            "CREATE_CHILD",
            "GENERIC_WRITE",
            "GENERIC_ALL",
            "WRITE_ATTRIBUTES",
            "WRITE_PROPERTIES",
            "WRITE_PROPERTY",
            "APPEND_DATA",
            "WRITE_DATA",
            "ALL_ACCESS",
            "DELETE_CHILD",
            "CREATE_CHILD",
            "WRITE_TREE",
            "FILE_WRITE",
            "FILE_ALL",
            "KEY_WRITE",
            "KEY_ALL",
            "STANDARD_RIGHTS_ALL",
            "STANDARD_DELETE",
            "DELETE_TREE",
            "ADD_FILE",
            "ADD_SUBDIRECTORY",
            "CREATE_PIPE_INSTANCE",
            "WRITE",
            "CREATE_LINK",
            "SET_VALUE",
            "WRITE_DAC",
            "WRITE_OWNER",
            "SET_VALUE"
        };
    }
}