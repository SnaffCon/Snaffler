using System.Collections.Generic;

namespace Sddl.Parser
{
    public class Sid : Acm
    {
        public string Raw { get; }

        public string Alias { get; }

        public Sid(string sid)
        {
            Raw = sid;

            string alias = Match.OneByRegexOrPrefix(sid, KnownAliasesList);

            if (alias == null)
            {
                Report(Error.SDP001.Format(sid));

                alias = Format.Unknown(sid);
            }

            Alias = alias;
        }

        /// <summary>
        /// A list of tuples of bigram in format XX as define in https://docs.microsoft.com/en-us/windows/win32/secauthz/sid-strings and
        /// well known SIDs in format S-* as defined in https://support.microsoft.com/en-us/help/243330/well-known-security-identifiers-in-windows-operating-systems.
        /// </summaty>
        internal static readonly List<(string bigram, string sid, string alias)> KnownAliasesList = new List<(string bigram, string sid, string alias)>
        {
            ( null, "^S-1-0$", @"Null Authority" ),
            ( null, "^S-1-0-0$", @"Nobody" ),
            ( null, "^S-1-1$", @"World Authority" ),
            ( "WD", "^S-1-1-0$", @"Everyone" ),
            ( null, "^S-1-2$", @"Local Authority" ),
            ( null, "^S-1-2-0$", @"Local" ),
            ( null, "^S-1-2-1$", @"Console Logon" ),
            ( null, "^S-1-3$", @"Creator Authority" ),
            ( "CO", "^S-1-3-0$", @"Creator Owner" ),
            ( "CG", "^S-1-3-1$", @"Creator Group" ),
            ( null, "^S-1-3-2$", @"Creator Owner Server" ),
            ( null, "^S-1-3-3$", @"Creator Group Server" ),
            ( "OW", "^S-1-3-4$", @"Owner Rights" ),
            ( null, "^S-1-4$", @"Non-unique Authority" ),
            ( null, "^S-1-5$", @"NT Authority" ),
            ( null, "^S-1-5-1$", @"Dialup" ),
            ( "NU", "^S-1-5-2$", @"Network" ),
            ( null, "^S-1-5-3$", @"Batch" ),
            ( "IU", "^S-1-5-4$", @"Interactive" ),
            ( null, "^S-1-5-5-(.+)-(.+)$", @"Logon Session" ),
            ( "SU", "^S-1-5-6$", @"Service" ),
            ( "AN", "^S-1-5-7$", @"Anonymous" ),
            ( null, "^S-1-5-8$", @"Proxy" ),
            ( "ED", "^S-1-5-9$", @"Enterprise Domain Controllers" ),
            ( "PS", "^S-1-5-10$", @"Principal Self" ),
            ( "AU", "^S-1-5-11$", @"Authenticated Users" ),
            ( "RC", "^S-1-5-12$", @"Restricted Code" ),
            ( null, "^S-1-5-13$", @"Terminal Server Users" ),
            ( null, "^S-1-5-14$", @"Remote Interactive Logon" ),
            ( null, "^S-1-5-15$", @"This Organization" ),
            ( null, "^S-1-5-17$", @"IUSR" ),
            ( "SY", "^S-1-5-18$", @"Local System" ),
            ( "LS", "^S-1-5-19$", @"Local Service" ),
            ( "NS", "^S-1-5-20$", @"Network Service" ),
            ( "LA", "^S-1-5-21(.*)-500$", @"Administrator" ),
            ( "LG", "^S-1-5-21(.*)-501$", @"Guest" ),
            ( null, "^S-1-5-21(.*)-502$", @"KRBTGT" ),
            ( "DA", "^S-1-5-21(.*)-512$", @"Domain Admins" ),
            ( "DU", "^S-1-5-21(.*)-513$", @"Domain Users" ),
            ( "DG", "^S-1-5-21(.*)-514$", @"Domain Guests" ),
            ( "DC", "^S-1-5-21(.*)-515$", @"Domain Computers" ),
            ( "DD", "^S-1-5-21(.*)-516$", @"Domain Controllers" ),
            ( "CA", "^S-1-5-21(.*)-517$", @"Cert Publishers" ),
            ( "SA", "^S-1-5-21(.*)-518$", @"Schema Admins" ),
            ( "EA", "^S-1-5-21(.*)-519$", @"Enterprise Admins" ),
            ( "PA", "^S-1-5-21(.*)-520$", @"Group Policy Creator Owners" ),
            ( "AP", "^S-1-5-21(.*)-525$", @"Protected Users" ),
            ( "KA", "^S-1-5-21(.*)-526$", @"Key Admins" ),
            ( "EK", "^S-1-5-21(.*)-527$", @"Enterprise Key Admins" ),
            ( "RS", "^S-1-5-21(.*)-553$", @"RAS and IAS Servers" ),
            ( "BA", "^S-1-5-32-544$", @"Administrators" ),
            ( "BU", "^S-1-5-32-545$", @"Users" ),
            ( "BG", "^S-1-5-32-546$", @"Guests" ),
            ( "PU", "^S-1-5-32-547$", @"Power Users" ),
            ( "AO", "^S-1-5-32-548$", @"Account Operators" ),
            ( "SO", "^S-1-5-32-549$", @"Server Operators" ),
            ( "PO", "^S-1-5-32-550$", @"Print Operators" ),
            ( "BO", "^S-1-5-32-551$", @"Backup Operators" ),
            ( "RE", "^S-1-5-32-552$", @"Replicators" ),
            ( "WR", "^S-1-5-33$", @"Write Restricted Code" ),
            ( null, "^S-1-5-64-10$", @"NTLM Authentication" ),
            ( null, "^S-1-5-64-14$", @"SChannel Authentication" ),
            ( null, "^S-1-5-64-21$", @"Digest Authentication" ),
            ( null, "^S-1-5-80$", @"NT Service" ),
            ( null, "^S-1-5-80-0$", @"All Services" ),
            ( null, "^S-1-5-83-0$", @"NT VIRTUAL MACHINE\Virtual Machines" ),
            ( null, "^S-1-16-0$", @"Untrusted Mandatory Level" ),
            ( null, "^S-1-16-4096$", @"Low Mandatory Level" ),
            ( null, "^S-1-16-8192$", @"Medium Mandatory Level" ),
            ( null, "^S-1-16-8448$", @"Medium Plus Mandatory Level" ),
            ( null, "^S-1-16-12288$", @"High Mandatory Level" ),
            ( null, "^S-1-16-16384$", @"System Mandatory Level" ),
            ( null, "^S-1-16-20480$", @"Protected Process Mandatory Level" ),
            ( null, "^S-1-16-28672$", @"Secure Process Mandatory Level" ),
            ( "RU", "^S-1-5-32-554$", @"BUILTIN\Pre-Windows 2000 Compatible Access" ),
            ( "RD", "^S-1-5-32-555$", @"BUILTIN\Remote Desktop Users" ),
            ( "NO", "^S-1-5-32-556$", @"BUILTIN\Network Configuration Operators" ),
            ( null, "^S-1-5-32-557$", @"BUILTIN\Incoming Forest Trust Builders" ),
            ( "MU", "^S-1-5-32-558$", @"BUILTIN\Performance Monitor Users" ),
            ( "LU", "^S-1-5-32-559$", @"BUILTIN\Performance Log Users" ),
            ( null, "^S-1-5-32-560$", @"BUILTIN\Windows Authorization Access Group" ),
            ( null, "^S-1-5-32-561$", @"BUILTIN\Terminal Server License Servers" ),
            ( null, "^S-1-5-32-562$", @"BUILTIN\Distributed COM Users" ),
            ( "RO", "^S-1-5-21(.*)-498$", @"Enterprise Read-only Domain Controllers" ),
            ( null, "^S-1-5-21(.*)-521$", @"Read-only Domain Controllers" ),
            ( "IS", "^S-1-5-32-568$", @"IIS_IUSRS" ),
            ( "CY", "^S-1-5-32-569$", @"BUILTIN\Cryptographic Operators" ),
            ( null, "^S-1-5-21(.*)-571$", @"Allowed RODC Password Replication Group" ),
            ( null, "^S-1-5-21(.*)-572$", @"Denied RODC Password Replication Group" ),
            ( "ER", "^S-1-5-32-573$", @"BUILTIN\Event Log Readers" ),
            ( "CD", "^S-1-5-32-574$", @"BUILTIN\Certificate Service DCOM Access" ),
            ( "CN", "^S-1-5-21(.*)-522$", @"Cloneable Domain Controllers" ),
            ( "RA", "^S-1-5-32-575$", @"BUILTIN\RDS Remote Access Servers" ),
            ( "ES", "^S-1-5-32-576$", @"BUILTIN\RDS Endpoint Servers" ),
            ( "MS", "^S-1-5-32-577$", @"BUILTIN\RDS Management Servers" ),
            ( "HA", "^S-1-5-32-578$", @"BUILTIN\Hyper-V Administrators" ),
            ( "AA", "^S-1-5-32-579$", @"BUILTIN\Access Control Assistance Operators" ),
            ( "RM", "^S-1-5-32-580$", @"BUILTIN\Remote Management Users" ),
            ( "UD", "^S-1-5-84-0-0-0-0-0$", @"User-Mode Driver Process" ),
            ( "AC", "^S-1-15-2-1$", @"All App Package" ),
            ( "AS", "^S-1-18-1$", @"Authentication Authority Asserted Identity" ),
            ( "SS", "^S-1-18-2$", @"Service Asserted Identity" )
        };

        public override string ToString()
        {
            return Alias.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is Sid sid &&
                   Alias == sid.Alias;
        }

        public static bool operator ==(Sid sid0, Sid sid1)
        {
            if (sid0 is null && sid1 is null)
                return true;
            else if (sid0 is null || sid1 is null)
                return false;
            else
                return sid0.Equals(sid1);
        }

        public static bool operator !=(Sid sid0, Sid sid1)
        {
            return !(sid0 == sid1);
        }
    }
}