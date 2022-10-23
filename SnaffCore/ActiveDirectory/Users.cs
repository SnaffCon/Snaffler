using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace SnaffCore.ActiveDirectory
{
    public class Trustee
    {
        public string Sid { get; set; }
        public string DisplayName { get; set; }
        public string DistinguishedName { get; set; }

        public Trustee() { }
        public Trustee(string input, bool isSid = true)
        {
            // clean up input
            input = input.Trim('*');
            if (isSid)
            {
                Sid = input;
                // try to resolve the sid
                try
                {
                    DisplayName = GetUserFromSid(input);
                }
                catch (Exception e)
                {
                    DisplayName = "Failed SID resolution";
                    //throw new UserException("Failed to resolve SID: " + input, e);
                }
            }
            else
            {
                DisplayName = input;
            }
        }

        public string GetUserFromSid(string sidString)
        {
            // stolen wholesale from http://www.pinvoke.net/default.aspx/advapi32.LookupAccountSid

            StringBuilder name = new StringBuilder();
            uint cchName = (uint)name.Capacity;
            StringBuilder referencedDomainName = new StringBuilder();
            uint cchReferencedDomainName = (uint)referencedDomainName.Capacity;
            SID_NAME_USE sidUse;
            int err = 0;
            try
            {
                SecurityIdentifier sidObj = new SecurityIdentifier(sidString);
                byte[] sidBytes = new byte[sidObj.BinaryLength];
                sidObj.GetBinaryForm(sidBytes, 0);
                err = NO_ERROR;

                if (!LookupAccountSid(null, sidBytes, name, ref cchName, referencedDomainName,
                    ref cchReferencedDomainName,
                    out sidUse))
                {
                    err = Marshal.GetLastWin32Error();
                    if (err == ERROR_INSUFFICIENT_BUFFER)
                    {
                        name.EnsureCapacity((int)cchName);
                        referencedDomainName.EnsureCapacity((int)cchReferencedDomainName);
                        err = NO_ERROR;
                        if (!LookupAccountSid(null, sidBytes, name, ref cchName, referencedDomainName,
                            ref cchReferencedDomainName, out sidUse))
                            err = Marshal.GetLastWin32Error();
                    }
                }
            }
            catch (Exception e)
            {
                return "lookupFailed";
            }

            string lookupResult = "";
            if (err != 0)
            {
                //BlockingMq.GetInstance().Trace(@"Error in SID Lookup : " + err + " resolving SID " + sidString + " handing off to well known sids list.");

                try
                {
                    lookupResult = GetWellKnownSid(sidString);
                }
                catch (Exception e)
                {
                    return "lookupFailed";
                }

                return lookupResult;
            }

            if (referencedDomainName.ToString().Length > 0)
            {
                lookupResult = referencedDomainName.ToString() + "\\" + name.ToString();
            }
            else
            {
                lookupResult = name.ToString();
            }

            return lookupResult;
        }

        public static string GetWellKnownSid(string sid)
        {
            bool isDomainSid = sid.StartsWith("S-1-5-21");

            Dictionary<string, string> sidDict = new Dictionary<string, string>
            {
                {"S-1-0", "Null Authority"},
                {"S-1-0-0", "Nobody"},
                {"S-1-1", "World Authority"},
                {"S-1-1-0", "Everyone"},
                {"S-1-2", "Local Authority"},
                {"S-1-2-0", "Local"},
                {"S-1-2-1", "Console Logon"},
                {"S-1-3", "Creator Authority"},
                {"S-1-3-0", "Creator Owner"},
                {"S-1-3-1", "Creator Group"},
                {"S-1-3-2", "Creator Owner Server"},
                {"S-1-3-3", "Creator Group Server"},
                {"S-1-3-4", "Owner Rights"},
                {"S-1-4", "Non-unique Authority"},
                {"S-1-5", "NT Authority"},
                {"S-1-5-1", "Dialup"},
                {"S-1-5-2", "Network"},
                {"S-1-5-3", "Batch"},
                {"S-1-5-4", "Interactive"},
                {"S-1-5-6", "Service"},
                {"S-1-5-7", "Anonymous"},
                {"S-1-5-8", "Proxy"},
                {"S-1-5-9", "Enterprise Domain Controllers"},
                {"S-1-5-10", "Principal Self"},
                {"S-1-5-11", "Authenticated Users"},
                {"S-1-5-12", "Restricted Code"},
                {"S-1-5-13", "Terminal Server Users"},
                {"S-1-5-14", "Remote Interactive Logon"},
                {"S-1-5-15", "This Organization"},
                {"S-1-5-17", "This Organization"},
                {"S-1-5-18", "Local System"},
                {"S-1-5-19", "NT Authority\\Local Service"},
                {"S-1-5-20", "NT Authority\\Network Service"},
                {"S-1-5-21-<DOMAIN>-498", "Enterprise Read-only Domain Controllers"},
                {"S-1-5-21-<DOMAIN>-500", "Administrator"},
                {"S-1-5-21-<DOMAIN>-501", "Guest"},
                {"S-1-5-21-<DOMAIN>-502", "KRBTGT"},
                {"S-1-5-21-<DOMAIN>-512", "Domain Admins"},
                {"S-1-5-21-<DOMAIN>-513", "Domain Users"},
                {"S-1-5-21-<DOMAIN>-514", "Domain Guests"},
                {"S-1-5-21-<DOMAIN>-515", "Domain Computers"},
                {"S-1-5-21-<DOMAIN>-516", "Domain Controllers"},
                {"S-1-5-21-<DOMAIN>-517", "Cert Publishers"},
                {"S-1-5-21-<DOMAIN>-518", "Schema Admins"},
                {"S-1-5-21-<DOMAIN>-519", "Enterprise Admins"},
                {"S-1-5-21-<DOMAIN>-520", "Group Policy Creator Owners"},
                {"S-1-5-21-<DOMAIN>-522", "Cloneable Domain Controllers"},
                {"S-1-5-21-<DOMAIN>-526", "Key Admins"},
                {"S-1-5-21-<DOMAIN>-527", "Enterprise Key Admins"},
                {"S-1-5-21-<DOMAIN>-553", "RAS and IAS Servers"},
                {"S-1-5-21-<DOMAIN>-521", "Read-only Domain Controllers"},
                {"S-1-5-21-<DOMAIN>-571", "Allowed RODC Password Replication Group"},
                {"S-1-5-21-<DOMAIN>-572", "Denied RODC Password Replication Group"},
                {"S-1-5-32-544", "Administrators"},
                {"S-1-5-32-545", "Users"},
                {"S-1-5-32-546", "Guests"},
                {"S-1-5-32-547", "Power Users"},
                {"S-1-5-32-548", "Account Operators"},
                {"S-1-5-32-549", "Server Operators"},
                {"S-1-5-32-550", "Print Operators"},
                {"S-1-5-32-551", "Backup Operators"},
                {"S-1-5-32-552", "Replicators"},
                {"S-1-5-64-10", "NTLM Authentication"},
                {"S-1-5-64-14", "SChannel Authentication"},
                {"S-1-5-64-21", "Digest Authentication"},
                {"S-1-5-80", "NT Service"},
                {"S-1-5-80-0", "All Services"},
                {"S-1-5-83-0", "NT VIRTUAL MACHINE\\Virtual Machines"},
                {"S-1-16-0", "Untrusted Mandatory Level"},
                {"S-1-16-4096", "Low Mandatory Level"},
                {"S-1-16-8192", "Medium Mandatory Level"},
                {"S-1-16-8448", "Medium Plus Mandatory Level"},
                {"S-1-16-12288", "High Mandatory Level"},
                {"S-1-16-16384", "System Mandatory Level"},
                {"S-1-16-20480", "Protected Process Mandatory Level"},
                {"S-1-16-28672", "Secure Process Mandatory Level"},
                {"S-1-5-32-554", "BUILTIN\\Pre-Windows 2000 Compatible Access"},
                {"S-1-5-32-555", "BUILTIN\\Remote Desktop Users"},
                {"S-1-5-32-556", "BUILTIN\\Network Configuration Operators"},
                {"S-1-5-32-557", "BUILTIN\\Incoming Forest Trust Builders"},
                {"S-1-5-32-558", "BUILTIN\\Performance Monitor Users"},
                {"S-1-5-32-559", "BUILTIN\\Performance Log Users"},
                {"S-1-5-32-560", "BUILTIN\\Windows Authorization Access Group"},
                {"S-1-5-32-561", "BUILTIN\\Terminal Server License Servers"},
                {"S-1-5-32-562", "BUILTIN\\Distributed COM Users"},
                {"S-1-5-32-573", "BUILTIN\\Event Log Readers"},
                {"S-1-5-32-574", "BUILTIN\\Certificate Service DCOM Access"},
                {"S-1-5-32-569", "BUILTIN\\Cryptographic Operators"},
                {"S-1-5-32-575", "BUILTIN\\RDS Remote Access Servers"},
                {"S-1-5-32-576", "BUILTIN\\RDS Endpoint Servers"},
                {"S-1-5-32-577", "BUILTIN\\RDS Management Servers"},
                {"S-1-5-32-578", "BUILTIN\\Hyper-V Administrators"},
                {"S-1-5-32-579", "BUILTIN\\Access Control Assistance Operators"},
                {"S-1-5-32-580", "BUILTIN\\Remote Management Users"}
            };

            foreach (KeyValuePair<string, string> trustee in sidDict)
            {
                if (isDomainSid)
                {
                    string[] splitSid = sid.Split('-');
                    string[] splitTrustee = trustee.Key.Split('-');
                    // if there's not at least 5 elements in the sid it's not a domain sid
                    if (splitTrustee.Length < 5)
                    {
                        continue;
                    }
                    if (splitSid[7] == splitTrustee[5])
                    {
                        return trustee.Value;
                    }
                }

                if (trustee.Key == sid)
                {
                    return trustee.Value;
                }
            }
            return "Failed to resolve SID.";
        }

        public static string GetWKSidHighOrLow(string sid)
        {
            bool isDomainSid = sid.StartsWith("S-1-5-21");

            Dictionary<string, string> sidDict = new Dictionary<string, string>
            {
                // TODO revise this list
                {"S-1-0",""}, //Null Authority
                {"S-1-0-0",""}, //Nobody
                {"S-1-1",""}, //World Authority
                {"S-1-1-0","Low"}, //Everyone
                {"S-1-2",""}, //Local Authority
                {"S-1-2-0",""}, //Local
                {"S-1-2-1","Low"}, //Console Logon
                {"S-1-3",""}, //Creator Authority
                {"S-1-3-0","High"}, //Creator Owner
                {"S-1-4",""}, //Non-unique Authority
                {"S-1-5",""}, //NT Authority
                {"S-1-5-1",""}, //Dialup
                {"S-1-5-2","Low"}, //Network
                {"S-1-5-3","Low"}, //Batch
                {"S-1-5-4","Low"}, //Interactive
                {"S-1-5-6",""}, //Service
                {"S-1-5-7","Low"}, //Anonymous
                {"S-1-5-9","High"}, //Enterprise Domain Controllers
                {"S-1-5-10",""}, //Principal Self
                {"S-1-5-11","Low"}, //Authenticated Users
                {"S-1-5-12",""}, //Restricted Code
                {"S-1-5-13","Low"}, //Terminal Server Users
                {"S-1-5-14","Low"}, //Remote Interactive Logon
                {"S-1-5-18","High"}, //Local System
                {"S-1-5-21-<DOMAIN>-498","High"}, //Enterprise Read-only Domain Controllers
                {"S-1-5-21-<DOMAIN>-500","High"}, //Administrator
                {"S-1-5-21-<DOMAIN>-501","Low"}, //Guest
                {"S-1-5-21-<DOMAIN>-502","High"}, //KRBTGT
                {"S-1-5-21-<DOMAIN>-512","High"}, //Domain Admins
                {"S-1-5-21-<DOMAIN>-513","Low"}, //Domain Users
                {"S-1-5-21-<DOMAIN>-514","Low"}, //Domain Guests
                {"S-1-5-21-<DOMAIN>-515","Low"}, //Domain Computers
                {"S-1-5-21-<DOMAIN>-516","High"}, //Domain Controllers
                {"S-1-5-21-<DOMAIN>-517",""}, //Cert Publishers
                {"S-1-5-21-<DOMAIN>-518","High"}, //Schema Admins
                {"S-1-5-21-<DOMAIN>-519","High"}, //Enterprise Admins
                {"S-1-5-21-<DOMAIN>-520","High"}, //Group Policy Creator Owners
                {"S-1-5-21-<DOMAIN>-522","High"}, //Cloneable Domain Controllers
                {"S-1-5-21-<DOMAIN>-526","High"}, //Key Admins
                {"S-1-5-21-<DOMAIN>-527","High"}, //Enterprise Key Admins
                {"S-1-5-21-<DOMAIN>-553",""}, //RAS and IAS Servers
                {"S-1-5-21-<DOMAIN>-521","High"}, //Read-only Domain Controllers
                {"S-1-5-21-<DOMAIN>-571",""}, //Allowed RODC Password Replication Group
                {"S-1-5-21-<DOMAIN>-572",""}, //Denied RODC Password Replication Group
                {"S-1-5-32-544","High"}, //Administrators
                {"S-1-5-32-545","Low"}, //Users
                {"S-1-5-32-546","Low"}, //Guests
                {"S-1-5-32-547",""}, //Power Users
                {"S-1-5-32-548",""}, //Account Operators
                {"S-1-5-32-549",""}, //Server Operators
                {"S-1-5-32-550",""}, //Print Operators
                {"S-1-5-32-551",""}, //Backup Operators
                {"S-1-5-32-552",""}, //Replicators
                {"S-1-5-64-10",""}, //NTLM Authentication
                {"S-1-5-64-14",""}, //SChannel Authentication
                {"S-1-5-64-21",""}, //Digest Authentication
                {"S-1-5-80",""}, //NT Service
                {"S-1-5-80-0",""}, //All Services
                {"S-1-5-83-0",""}, //NT VIRTUAL MACHINE\\Virtual Machines
                {"S-1-5-32-554","Low"}, //BUILTIN\\Pre-Windows 2000 Compatible Access
                {"S-1-5-32-555",""}, //BUILTIN\\Remote Desktop Users
                {"S-1-5-32-556",""}, //BUILTIN\\Network Configuration Operators
                {"S-1-5-32-557",""}, //BUILTIN\\Incoming Forest Trust Builders
                {"S-1-5-32-558",""}, //BUILTIN\\Performance Monitor Users
                {"S-1-5-32-559",""}, //BUILTIN\\Performance Log Users
                {"S-1-5-32-560",""}, //BUILTIN\\Windows Authorization Access Group
                {"S-1-5-32-561",""}, //BUILTIN\\Terminal Server License Servers
                {"S-1-5-32-562",""}, //BUILTIN\\Distributed COM Users
                {"S-1-5-32-573",""}, //BUILTIN\\Event Log Readers
                {"S-1-5-32-574",""}, //BUILTIN\\Certificate Service DCOM Access
                {"S-1-5-32-569",""}, //BUILTIN\\Cryptographic Operators
                {"S-1-5-32-575",""}, //BUILTIN\\RDS Remote Access Servers
                {"S-1-5-32-576",""}, //BUILTIN\\RDS Endpoint Servers
                {"S-1-5-32-577",""}, //BUILTIN\\RDS Management Servers
                {"S-1-5-32-578","High"}, //BUILTIN\\Hyper-V Administrators
                {"S-1-5-32-579",""}, //BUILTIN\\Access Control Assistance Operators
                {"S-1-5-32-580",""}, //BUILTIN\\Remote Management Users"}
            };
            try
            {
                foreach (KeyValuePair<string, string> trustee in sidDict)
                {
                    if (isDomainSid)
                    {
                        string[] splitSid = sid.Split('-');
                        string[] splitTrustee = trustee.Key.Split('-');
                        if ((splitSid.Length > 4) && (splitTrustee.Length > 4))
                            if (splitSid[4] == splitTrustee[4])
                            {
                                return trustee.Value;
                            }
                    }

                    if (trustee.Key == sid)
                    {
                        return trustee.Value;
                    }
                }
            }
            catch (Exception e)
            {
                return "failedToResolve";
            }

            return "";
        }

        const int NO_ERROR = 0;
        const int ERROR_INSUFFICIENT_BUFFER = 122;

        enum SID_NAME_USE
        {
            SidTypeUser = 1,
            SidTypeGroup,
            SidTypeDomain,
            SidTypeAlias,
            SidTypeWellKnownGroup,
            SidTypeDeletedAccount,
            SidTypeInvalid,
            SidTypeUnknown,
            SidTypeComputer
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool LookupAccountSid(
            string lpSystemName,
            [MarshalAs(UnmanagedType.LPArray)] byte[] Sid,
            StringBuilder lpName,
            ref uint cchName,
            StringBuilder referencedDomainName,
            ref uint cchReferencedDomainName,
            out SID_NAME_USE peUse);
    }
}
