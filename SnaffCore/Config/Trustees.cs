using System.Collections.Generic;
using SnaffCore.Config;

namespace SnaffCore.Config
{
    public partial class AclOptions
    {
        public void LoadTrusteeOptions()
        {
            TrusteeOptions = new List<TrusteeOption>()
            {
                new TrusteeOption()
                {
                    SID = "S-1-0",
                    DisplayName = "Null Authority",
                    Description = "An identifier authority.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-0-0",
                    DisplayName = "Nobody",
                    Description = "No security principal.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-1",
                    DisplayName = "World Authority",
                    Description = "An identifier authority.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-1-0",
                    DisplayName = "Everyone",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-2",
                    DisplayName = "Local Authority",
                    Description = "An identifier authority.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-2-0",
                    DisplayName = "Local",
                    Description = "A group that includes all users who have logged on locally.",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-2-1",
                    DisplayName = "Console Logon",
                    Description = "A group that includes users who are logged on to the physical console.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-3",
                    DisplayName = "Creator Authority",
                    Description = "An identifier authority.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-3-0",
                    DisplayName = "Creator Owner",
                    Description =
                        "A placeholder in an inheritable access control entry (ACE). When the ACE is inherited, the system replaces this SID with the SID for the object's creator.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-3-1",
                    DisplayName = "Creator Group",
                    Description =
                        "A placeholder in an inheritable ACE. When the ACE is inherited, the system replaces this SID with the SID for the primary group of the object's creator. The primary group is used only by the POSIX subsystem.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-3-2",
                    DisplayName = "Creator Owner Server",
                    Description = "This SID is not used in Windows 2000.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-3-3",
                    DisplayName = "Creator Group Server",
                    Description = "This SID is not used in Windows 2000.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-3-4",
                    DisplayName = "Owner Rights",
                    Description =
                        "A group that represents the current owner of the object. When an ACE that carries this SID is applied to an object, the system ignores the implicit READ_CONTROL and WRITE_DAC permissions for the object owner.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-80-0",
                    DisplayName = "All Services",
                    Description =
                        "A group that includes all service processes configured on the system. Membership is controlled by the operating system.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-4",
                    DisplayName = "Non-unique Authority",
                    Description = "An identifier authority.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5",
                    DisplayName = "NT Authority",
                    Description = "An identifier authority.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-1",
                    DisplayName = "Dialup",
                    Description =
                        "A group that includes all users who have logged on through a dial-up connection. Membership is controlled by the operating system.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-2",
                    DisplayName = "Network",
                    Description =
                        "A group that includes all users that have logged on through a network connection. Membership is controlled by the operating system.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-3",
                    DisplayName = "Batch",
                    Description =
                        "A group that includes all users that have logged on through a batch queue facility. Membership is controlled by the operating system.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-4",
                    DisplayName = "Interactive",
                    Description =
                        "A group that includes all users that have logged on interactively. Membership is controlled by the operating system.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-6",
                    DisplayName = "Service",
                    Description =
                        "A group that includes all security principals that have logged on as a service. Membership is controlled by the operating system.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-7",
                    DisplayName = "Anonymous",
                    Description =
                        "A group that includes all users that have logged on anonymously. Membership is controlled by the operating system.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-8",
                    DisplayName = "Proxy",
                    Description = "This SID is not used in Windows 2000.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-9",
                    DisplayName = "Enterprise Domain Controllers",
                    Description =
                        "A group that includes all domain controllers in a forest that uses an Active Directory directory service. Membership is controlled by the operating system.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-10",
                    DisplayName = "Principal Self",
                    Description =
                        "A placeholder in an inheritable ACE on an account object or group object in Active Directory. When the ACE is inherited, the system replaces this SID with the SID for the security principal who holds the account.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-11",
                    DisplayName = "Authenticated Users",
                    Description =
                        "A group that includes all users whose identities were authenticated when they logged on. Membership is controlled by the operating system.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-12",
                    DisplayName = "Restricted Code",
                    Description = "This SID is reserved for future use.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-13",
                    DisplayName = "Terminal Server Users",
                    Description =
                        "A group that includes all users that have logged on to a Terminal Services server. Membership is controlled by the operating system.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-14",
                    DisplayName = "Remote Interactive Logon",
                    Description =
                        "A group that includes all users who have logged on through a terminal services logon.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-15",
                    DisplayName = "This Organization",
                    Description =
                        "A group that includes all users from the same organization. Only included with AD accounts and only added by a Windows Server 2003 or later domain controller.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-17",
                    DisplayName = "This Organization",
                    Description = "An account that is used by the default Internet Information Services (IIS) user.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-18",
                    DisplayName = "Local System",
                    Description = "A service account that is used by the operating system.",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-19",
                    DisplayName = "NT Authority\\Local Service",
                    Description = "Local Service",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-20",
                    DisplayName = "NT Authority\\Network Service",
                    Description = "Network Service",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-21-<DOMAIN>-500",
                    DisplayName = "Administrator",
                    Description =
                        "A user account for the system administrator. By default, it is the only user account that is given full control over the system.",
                    DomainSID = true,
                    LocalSID = false,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-21-<DOMAIN>-500",
                    DisplayName = "BUILTIN\\Administrator",
                    Description =
                        "A user account for the system administrator. By default, it is the only user account that is given full control over the system.",
                    DomainSID = true,
                    LocalSID = false,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-21-<DOMAIN>-501",
                    DisplayName = "Guest",
                    Description =
                        "A user account for people who do not have individual accounts. This user account does not require a password. By default, the Guest account is disabled.",
                    DomainSID = true,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                 new TrusteeOption()
                {
                    SID = "S-1-5-21-<DOMAIN>-501",
                    DisplayName = "BUILTIN\\Guest",
                    Description =
                        "A user account for people who do not have individual accounts. This user account does not require a password. By default, the Guest account is disabled.",
                    DomainSID = true,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-21-<DOMAIN>-502",
                    DisplayName = "KRBTGT",
                    Description = "A service account that is used by the Key Distribution Center (KDC) service.",
                    DomainSID = true,
                    LocalSID = false,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-21-<DOMAIN>-512",
                    DisplayName = "Domain Admins",
                    Description =
                        "A global group whose members are authorized to administer the domain. By default, the Domain Admins group is a member of the Administrators group on all computers that have joined a domain, including the domain controllers. Domain Admins is the default owner of any object that is created by any member of the group.",
                    DomainSID = true,
                    LocalSID = false,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-21-<DOMAIN>-513",
                    DisplayName = "Domain Users",
                    Description =
                        "A global group that, by default, includes all user accounts in a domain. When you create a user account in a domain, it is added to this group by default.",
                    DomainSID = true,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-21-<DOMAIN>-514",
                    DisplayName = "Domain Guests",
                    Description =
                        "A global group that, by default, has only one member, the domain's built-in Guest account.",
                    DomainSID = true,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-21-<DOMAIN>-515",
                    DisplayName = "Domain Computers",
                    Description = "A global group that includes all clients and servers that have joined the domain.",
                    DomainSID = true,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-21-<DOMAIN>-516",
                    DisplayName = "Domain Controllers",
                    Description =
                        "A global group that includes all domain controllers in the domain. New domain controllers are added to this group by default.",
                    DomainSID = true,
                    LocalSID = false,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-21-<DOMAIN>-517",
                    DisplayName = "Cert Publishers",
                    Description =
                        "A global group that includes all computers that are running an enterprise certification authority. Cert Publishers are authorized to publish certificates for User objects in Active Directory.",
                    DomainSID = true,
                    LocalSID = false,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-21-<DOMAIN>-518",
                    DisplayName = "Schema Admins",
                    Description =
                        "A universal group in a native-mode domain; a global group in a mixed-mode domain. The group is authorized to make schema changes in Active Directory. By default, the only member of the group is the Administrator account for the forest root domain.",
                    DomainSID = true,
                    LocalSID = false,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-21-<DOMAIN>-519",
                    DisplayName = "Enterprise Admins",
                    Description =
                        "A universal group in a native-mode domain; a global group in a mixed-mode domain. The group is authorized to make forest-wide changes in Active Directory, such as adding child domains. By default, the only member of the group is the Administrator account for the forest root domain.",
                    DomainSID = true,
                    LocalSID = false,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-21-<DOMAIN>-520",
                    DisplayName = "Group Policy Creator Owners",
                    Description =
                        "A global group that is authorized to create new Group Policy objects in Active Directory. By default, the only member of the group is Administrator.",
                    DomainSID = true,
                    LocalSID = false,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-21-<DOMAIN>-526",
                    DisplayName = "Key Admins",
                    Description =
                        "A security group. The intention for this group is to have delegated write access on the msdsKeyCredentialLink attribute only. The group is intended for use in scenarios where trusted external authorities (for example, Active Directory Federated Services) are responsible for modifying this attribute. Only trusted administrators should be made a member of this group.",
                    DomainSID = true,
                    LocalSID = false,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-21-<DOMAIN>-527",
                    DisplayName = "Enterprise Key Admins",
                    Description =
                        "A security group. The intention for this group is to have delegated write access on the msdsKeyCredentialLink attribute only. The group is intended for use in scenarios where trusted external authorities (for example, Active Directory Federated Services) are responsible for modifying this attribute. Only trusted administrators should be made a member of this group.",
                    DomainSID = true,
                    LocalSID = false,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-21-<DOMAIN>-553",
                    DisplayName = "RAS and IAS Servers",
                    Description =
                        "A domain local group. By default, this group has no members. Servers in this group have Read Account Restrictions and Read Logon Information access to User objects in the Active Directory domain local group.",
                    DomainSID = true,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-544",
                    DisplayName = "Administrators",
                    Description =
                        "A built-in group. After the initial installation of the operating system, the only member of the group is the Administrator account. When a computer joins a domain, the Domain Admins group is added to the Administrators group. When a server becomes a domain controller, the Enterprise Admins group also is added to the Administrators group.",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-544",
                    DisplayName = "BUILTIN\\Administrators",
                    Description =
                        "A built-in group. After the initial installation of the operating system, the only member of the group is the Administrator account. When a computer joins a domain, the Domain Admins group is added to the Administrators group. When a server becomes a domain controller, the Enterprise Admins group also is added to the Administrators group.",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-545",
                    DisplayName = "Users",
                    Description =
                        "A built-in group. After the initial installation of the operating system, the only member is the Authenticated Users group. When a computer joins a domain, the Domain Users group is added to the Users group on the computer.",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-545",
                    DisplayName = "BUILTIN\\Users",
                    Description =
                        "A built-in group. After the initial installation of the operating system, the only member is the Authenticated Users group. When a computer joins a domain, the Domain Users group is added to the Users group on the computer.",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-546",
                    DisplayName = "Guests",
                    Description =
                        "A built-in group. By default, the only member is the Guest account. The Guests group allows occasional or one-time users to log on with limited privileges to a computer's built-in Guest account.",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-546",
                    DisplayName = "BUILTIN\\Guests",
                    Description =
                        "A built-in group. By default, the only member is the Guest account. The Guests group allows occasional or one-time users to log on with limited privileges to a computer's built-in Guest account.",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-547",
                    DisplayName = "Power Users",
                    Description =
                        "A built-in group. By default, the group has no members. Power users can create local users and groups; modify and delete accounts that they have created; and remove users from the Power Users, Users, and Guests groups. Power users also can install programs; create, manage, and delete local printers; and create and delete file shares.",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-547",
                    DisplayName = "BUILTIN\\Power Users",
                    Description =
                        "A built-in group. By default, the group has no members. Power users can create local users and groups; modify and delete accounts that they have created; and remove users from the Power Users, Users, and Guests groups. Power users also can install programs; create, manage, and delete local printers; and create and delete file shares.",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-548",
                    DisplayName = "Account Operators",
                    Description =
                        "A built-in group that exists only on domain controllers. By default, the group has no members. By default, Account Operators have permission to create, modify, and delete accounts for users, groups, and computers in all containers and organizational units of Active Directory except the Builtin container and the Domain Controllers OU. Account Operators do not have permission to modify the Administrators and Domain Admins groups, nor do they have permission to modify the accounts for members of those groups.",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-548",
                    DisplayName = "BUILTIN\\Account Operators",
                    Description =
                        "A built-in group that exists only on domain controllers. By default, the group has no members. By default, Account Operators have permission to create, modify, and delete accounts for users, groups, and computers in all containers and organizational units of Active Directory except the Builtin container and the Domain Controllers OU. Account Operators do not have permission to modify the Administrators and Domain Admins groups, nor do they have permission to modify the accounts for members of those groups.",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-549",
                    DisplayName = "Server Operators",
                    Description =
                        "A built-in group that exists only on domain controllers. By default, the group has no members. Server Operators can log on to a server interactively; create and delete network shares; start and stop services; back up and restore files; format the hard disk of the computer; and shut down the computer.",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-549",
                    DisplayName = "BUILTIN\\Server Operators",
                    Description =
                        "A built-in group that exists only on domain controllers. By default, the group has no members. Server Operators can log on to a server interactively; create and delete network shares; start and stop services; back up and restore files; format the hard disk of the computer; and shut down the computer.",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-550",
                    DisplayName = "Print Operators",
                    Description =
                        "A built-in group that exists only on domain controllers. Print Operators can manage printers and document queues.",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-550",
                    DisplayName = "BUILTIN\\Print Operators",
                    Description =
                        "A built-in group that exists only on domain controllers. Print Operators can manage printers and document queues.",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-551",
                    DisplayName = "Backup Operators",
                    Description =
                        "A built-in group. By default, the group has no members. Backup Operators can back up and restore all files on a computer, regardless of the permissions that protect those files. Backup Operators also can log on to the computer and shut it down.",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-551",
                    DisplayName = "BUILTIN\\Backup Operators",
                    Description =
                        "A built-in group. By default, the group has no members. Backup Operators can back up and restore all files on a computer, regardless of the permissions that protect those files. Backup Operators also can log on to the computer and shut it down.",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-552",
                    DisplayName = "Replicators",
                    Description =
                        "A built-in group that is used by the File Replication service on domain controllers. By default, the group has no members. Do not add users to this group.",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-552",
                    DisplayName = "BUILTIN\\Replicators",
                    Description =
                        "A built-in group that is used by the File Replication service on domain controllers. By default, the group has no members. Do not add users to this group.",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-64-10",
                    DisplayName = "NTLM Authentication",
                    Description = "A SID that is used when the NTLM authentication package authenticated the client",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-64-14",
                    DisplayName = "SChannel Authentication",
                    Description =
                        "A SID that is used when the SChannel authentication package authenticated the client.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-64-21",
                    DisplayName = "Digest Authentication",
                    Description = "A SID that is used when the Digest authentication package authenticated the client.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-80",
                    DisplayName = "NT Service",
                    Description = "An NT Service account prefix",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-80-0",
                    DisplayName = "All Services",
                    Description =
                        "A group that includes all service processes that are configured on the system. Membership is controlled by the operating system.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-83-0",
                    DisplayName = "NT VIRTUAL MACHINE\\Virtual Machines",
                    Description =
                        "A built-in group. The group is created when the Hyper-V role is installed. Membership in the group is maintained by the Hyper-V Management Service (VMMS). This group requires the \"Create Symbolic Links\" right (SeCreateSymbolicLinkPrivilege), and also the \"Log on as a Service\" right (SeServiceLogonRight).",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-16-0",
                    DisplayName = "Untrusted Mandatory Level",
                    Description = "An untrusted integrity level. Note Added in Windows Vista and Windows Server 2008",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-16-4096",
                    DisplayName = "Low Mandatory Level",
                    Description = "A low integrity level.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-16-8192",
                    DisplayName = "Medium Mandatory Level",
                    Description = "A medium integrity level.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-16-8448",
                    DisplayName = "Medium Plus Mandatory Level",
                    Description = "A medium plus integrity level.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-16-12288",
                    DisplayName = "High Mandatory Level",
                    Description = "A high integrity level.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-16-16384",
                    DisplayName = "System Mandatory Level",
                    Description = "A system integrity level.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-16-20480",
                    DisplayName = "Protected Process Mandatory Level",
                    Description = "A protected-process integrity level.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-16-28672",
                    DisplayName = "Secure Process Mandatory Level",
                    Description = "A secure process integrity level.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-554",
                    DisplayName = "BUILTIN\\Pre-Windows 2000 Compatible Access",
                    Description =
                        "An alias added by Windows 2000. A backward compatibility group which allows read access on all users and groups in the domain.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = true
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-555",
                    DisplayName = "BUILTIN\\Remote Desktop Users",
                    Description = "An alias. Members in this group are granted the right to logon remotely.",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-556",
                    DisplayName = "BUILTIN\\Network Configuration Operators",
                    Description =
                        "An alias. Members in this group can have some administrative privileges to manage configuration of networking features.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-557",
                    DisplayName = "BUILTIN\\Incoming Forest Trust Builders",
                    Description = "An alias. Members of this group can create incoming, one-way trusts to this forest.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-558",
                    DisplayName = "BUILTIN\\Performance Monitor Users",
                    Description = "An alias. Members of this group have remote access to monitor this computer.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-559",
                    DisplayName = "BUILTIN\\Performance Log Users",
                    Description =
                        "An alias. Members of this group have remote access to schedule logging of performance counters on this computer.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-560",
                    DisplayName = "BUILTIN\\Windows Authorization Access Group",
                    Description =
                        "An alias. Members of this group have access to the computed tokenGroupsGlobalAndUniversal attribute on User objects.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-561",
                    DisplayName = "BUILTIN\\Terminal Server License Servers",
                    Description =
                        "An alias. A group for Terminal Server License Servers. When Windows Server 2003 Service Pack 1 is installed, a new local group is created.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-562",
                    DisplayName = "BUILTIN\\Distributed COM Users",
                    Description =
                        "An alias. A group for COM to provide computerwide access controls that govern access to all call, activation, or launch requests on the computer.",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-21-<DOMAIN>-498",
                    DisplayName = "Enterprise Read-only Domain Controllers",
                    Description =
                        "A Universal group. Members of this group are Read-Only Domain Controllers in the enterprise",
                    DomainSID = true,
                    LocalSID = false,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-21-<DOMAIN>-521",
                    DisplayName = "Read-only Domain Controllers",
                    Description =
                        "A Global group. Members of this group are Read-Only Domain Controllers in the domain",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-569",
                    DisplayName = "BUILTIN\\Cryptographic Operators",
                    Description = "A Builtin Local group. Members are authorized to perform cryptographic operations.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-21-<DOMAIN>-571",
                    DisplayName = "Allowed RODC Password Replication Group",
                    Description =
                        "A Domain Local group. Members in this group can have their passwords replicated to all read-only domain controllers in the domain.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-21-<DOMAIN>-572",
                    DisplayName = "Denied RODC Password Replication Group",
                    Description =
                        "A Domain Local group. Members in this group cannot have their passwords replicated to any read-only domain controllers in the domain",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-573",
                    DisplayName = "BUILTIN\\Event Log Readers",
                    Description =
                        "A Builtin Local group. Members of this group can read event logs from local machine.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-574",
                    DisplayName = "BUILTIN\\Certificate Service DCOM Access",
                    Description =
                        "A Builtin Local group. Members of this group are allowed to connect to Certification Authorities in the enterprise.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-21-<DOMAIN>-522",
                    DisplayName = "Cloneable Domain Controllers",
                    Description = "A Global group. Members of this group that are domain controllers may be cloned.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-575",
                    DisplayName = "BUILTIN\\RDS Remote Access Servers",
                    Description =
                        "A Builtin Local group. Servers in this group enable users of RemoteApp programs and personal virtual desktops access to these resources. In Internet-facing deployments, these servers are typically deployed in an edge network. This group needs to be populated on servers running RD Connection Broker. RD Gateway servers and RD Web Access servers used in the deployment need to be in this group.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-576",
                    DisplayName = "BUILTIN\\RDS Endpoint Servers",
                    Description =
                        "A Builtin Local group. Servers in this group run virtual machines and host sessions where users RemoteApp programs and personal virtual desktops run. This group needs to be populated on servers running RD Connection Broker. RD Session Host servers and RD Virtualization Host servers used in the deployment need to be in this group.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-577",
                    DisplayName = "BUILTIN\\RDS Management Servers",
                    Description =
                        "A Builtin Local group. Servers in this group can perform routine administrative actions on servers running Remote Desktop Services. This group needs to be populated on all servers in a Remote Desktop Services deployment. The servers running the RDS Central Management service must be included in this group.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-578",
                    DisplayName = "BUILTIN\\Hyper-V Administrators",
                    Description =
                        "A Builtin Local group. Members of this group have complete and unrestricted access to all features of Hyper-V.",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = true,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-579",
                    DisplayName = "BUILTIN\\Access Control Assistance Operators",
                    Description =
                        "A Builtin Local group. Members of this group can remotely query authorization attributes and permissions for resources on this computer.",
                    DomainSID = false,
                    LocalSID = false,
                    HighPriv = false,
                    LowPriv = false
                },
                new TrusteeOption()
                {
                    SID = "S-1-5-32-580",
                    DisplayName = "BUILTIN\\Remote Management Users",
                    Description =
                        "A Builtin Local group. Members of this group can access WMI resources over management protocols (such as WS-Management via the Windows Remote Management service). This applies only to WMI namespaces that grant access to the user.",
                    DomainSID = false,
                    LocalSID = true,
                    HighPriv = true,
                    LowPriv = false
                },
            };
        }
    }
}
