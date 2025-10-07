using System.Collections.Generic;

namespace SnaffCore.ActiveDirectory.LDAP
{
    class Helpers
    {

    private static readonly HashSet<string> Groups = new HashSet<string> { "268435456", "268435457", "536870912", "536870913" };
        private static readonly HashSet<string> Computers = new HashSet<string> { "805306369" };
        private static readonly HashSet<string> Users = new HashSet<string> { "805306368" };
        internal static LdapTypeEnum SamAccountTypeToType(string samAccountType)
        {
            if (Groups.Contains(samAccountType))
                return LdapTypeEnum.Group;

            if (Users.Contains(samAccountType))
                return LdapTypeEnum.User;

            if (Computers.Contains(samAccountType))
                return LdapTypeEnum.Computer;

            return LdapTypeEnum.Unknown;
        }
    }
}
