using System;
using SnaffCore.Concurrency;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using SnaffCore.ActiveDirectory.LDAP;
using static SnaffCore.Config.Options;
using System.DirectoryServices.Protocols;
using System.Linq;

namespace SnaffCore.ActiveDirectory
{
    public class AdData
    {
        private List<string> _domainComputers = new List<string>();
        private List<string> _domainUsers = new List<string>();
        private List<DFSShare> _dfsShares = new List<DFSShare>();
        private List<string> _dfsNamespacePaths;
//        private Domain _currentDomain;
//        private string _domainName;
        private Domain _targetDomain;
        private string _targetDc;
        private string _targetDomainNetBIOSName;
        private BlockingMq Mq { get; set; }

        public List<string> GetDomainComputers()
        {
            return _domainComputers;
        }

        public List<string> GetDomainUsers()
        {
            return _domainUsers;
        }

        public List<DFSShare> GetDfsShares()
        {
            return _dfsShares;
        }
/*
        public string GetDomainName()
        {
            return _domainName;
        }
*/
        public List<string> GetDfsNamespacePaths()
        {
            return _dfsNamespacePaths;
        }

        public DirectoryContext DirectoryContext { get; set; }
        private List<string> DomainControllers { get; set; } = new List<string>();

        private string GetNetBiosDomainName(string dnsDomainName)
        {
            string netbiosDomainName = string.Empty;

            DirectoryEntry rootDSE = new DirectoryEntry(String.Format("LDAP://{0}/RootDSE",dnsDomainName));

            string configurationNamingContext = rootDSE.Properties["configurationNamingContext"][0].ToString();

            DirectoryEntry searchRoot = new DirectoryEntry(String.Format("LDAP://{0}/cn=Partitions,{1}",dnsDomainName,configurationNamingContext));

            DirectorySearcher searcher = new DirectorySearcher(searchRoot);
            searcher.SearchScope = System.DirectoryServices.SearchScope.OneLevel;
            searcher.PropertiesToLoad.Add("netbiosname");
            searcher.Filter = string.Format("(&(objectcategory=Crossref)(dnsRoot={0})(netBIOSName=*))", dnsDomainName);

            SearchResult result = searcher.FindOne();

            if (result != null)
            {
                netbiosDomainName = result.Properties["netbiosname"][0].ToString();
            }

            return netbiosDomainName;
        }


        private DirectorySearch GetDirectorySearcher()
        {
            Mq = BlockingMq.GetMq();

            // target domain and dc set
            if ((!string.IsNullOrEmpty(MyOptions.TargetDomain)) && (!string.IsNullOrEmpty(MyOptions.TargetDc)))
            {
                Mq.Trace("Target DC and Domain specified: " + MyOptions.TargetDomain + " + " + MyOptions.TargetDc);
                _targetDc = MyOptions.TargetDc;
                _targetDomain = Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain, MyOptions.TargetDomain));
            }
            // no target DC or domain set
            else
            {
                Mq.Trace("Getting current domain from user context.");
//                _currentDomain = Domain.GetCurrentDomain();
//                _targetDomain = _currentDomain.Name;
                _targetDomain = Domain.GetCurrentDomain();
                _targetDc = _targetDomain.PdcRoleOwner.Name;
            }



            _targetDomainNetBIOSName = GetNetBiosDomainName(_targetDomain.Name);
            return new DirectorySearch(_targetDomain.Name, _targetDc);
        }

        public void SetDomainComputers(string LdapFilter)
        {
            DirectorySearch ds = GetDirectorySearcher();

            List<string> domainComputers = new List<string>();

            try
            {
                Mq.Degub("Starting DFS Enumeration.");

                DfsFinder dfsFinder = new DfsFinder();
                List<DFSShare> dfsShares = dfsFinder.FindDfsShares(ds);
                _dfsNamespacePaths = new List<string>();
                foreach (DFSShare dfsShare in dfsShares)
                {
                    string dfsShareNamespacePath = @"\\" + _targetDomain + @"\" + dfsShare.DFSNamespace;
                    dfsShare.DfsNamespacePath = dfsShareNamespacePath;
                    if (!_dfsNamespacePaths.Contains(dfsShareNamespacePath))
                    {
                        _dfsNamespacePaths.Add(dfsShareNamespacePath);
                    }
                    _dfsShares.Add(dfsShare);
                }
                Mq.Info("Found " + _dfsShares.Count.ToString() + " DFS Shares in " + _dfsNamespacePaths.Count.ToString() + " namespaces.");


                Mq.Degub("Finished DFS Enumeration.");
                if (!MyOptions.DfsOnly)
                {
                    // if limiting to DFS shares, we stop there.

                    string[] ldapProperties = new string[] { "name", "dNSHostName", "lastLogonTimeStamp" };
                    string ldapFilter = LdapFilter;

                    IEnumerable<SearchResultEntry> searchResultEntries = ds.QueryLdap(ldapFilter, ldapProperties, System.DirectoryServices.Protocols.SearchScope.Subtree);

                    foreach (SearchResultEntry resEnt in searchResultEntries)
                    {
                        int uacFlags;
                        bool success =
                            int.TryParse(resEnt.GetProperty("userAccountControl"),
                                out uacFlags);

                        UserAccountControlFlags userAccFlags = (UserAccountControlFlags)uacFlags;

                        if (userAccFlags.HasFlag(UserAccountControlFlags.AccountDisabled))
                        {
                            continue;
                        }

                        if (!String.IsNullOrEmpty(resEnt.GetProperty("dNSHostName")))
                        {
                            string computerName = resEnt.GetProperty("dNSHostName");
                            domainComputers.Add(computerName);
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Mq.Trace(e.ToString());
            }

            this._domainComputers = domainComputers;
        }

        public void SetDomainUsers()
        {
            DirectorySearch ds = GetDirectorySearcher();
            List<string> domainUsers = new List<string>();

            string[] ldapProperties = new string[] { "name", "adminCount", "sAMAccountName", "userAccountControl","servicePrincipalName","userPrincipalName"};
            string ldapFilter = "(&(objectClass=user)(objectCategory=person))";

            IEnumerable<SearchResultEntry> searchResultEntries = ds.QueryLdap(ldapFilter, ldapProperties, System.DirectoryServices.Protocols.SearchScope.Subtree);

            foreach (SearchResultEntry resEnt in searchResultEntries)
            {
                bool keepUser = false;
                try
                {                    
                    //busted account name
                    if (String.IsNullOrEmpty(resEnt.GetProperty("sAMAccountName")))
                    {
                        continue;
                    }

                    int uacFlags;
                    bool success =
                        int.TryParse(resEnt.GetProperty("userAccountControl"),
                            out uacFlags);
                    UserAccountControlFlags userAccFlags = (UserAccountControlFlags)uacFlags;

                    if (userAccFlags.HasFlag(UserAccountControlFlags.AccountDisabled))
                    {
                        continue;
                    }

                    string userName = resEnt.GetProperty("sAMAccountName");

                    if (userName.EndsWith("$"))
                    {
                        Mq.Trace("Skipping " + userName +
                                " because it appears to be a computer or trust account.");
                        continue;
                    }

                    //skip mailboxy accounts - domains always have a billion of these.
                    if (userName.IndexOf("mailbox", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Mq.Trace("Skipping " + userName +
                                " because it appears to be a mailbox.");
                        continue;
                    }

                    if (userName.IndexOf("mbx", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Mq.Trace("Skipping " + userName +
                                " because it appears to be a mailbox.");
                        continue;
                    }

                    // if has an SPN, keep it
                    if (!keepUser && resEnt.GetProperty("servicePrincipalName") != null)
                    {
                        Mq.Trace("Adding " + userName +
                                    " to target list because it has an SPN");
                        keepUser = true;
                    }

                    // if it's got adminCount, keep it
                    if (!keepUser && resEnt.GetProperty("adminCount") == "1")
                    {
                        Mq.Trace("Adding " + userName +
                                    " to target list because it had adminCount=1.");
                        keepUser = true;
                    }

                    // if the password doesn't expire it's probably a service account
                    if (!keepUser && userAccFlags.HasFlag(UserAccountControlFlags.PasswordDoesNotExpire))
                    {
                        Mq.Trace("Adding " + userName +
                                    " to target list because password does not expire,  probably service account.");
                        keepUser = true;
                    }

                    if (!keepUser && userAccFlags.HasFlag(UserAccountControlFlags.DontRequirePreauth))
                    {
                        Mq.Trace("Adding " + userName +
                                    " to target list because it doesn't require Kerberos pre-auth.");
                        keepUser = true;
                    }

                    if (!keepUser && userAccFlags.HasFlag(UserAccountControlFlags.TrustedForDelegation))
                    {
                        Mq.Trace("Adding " + userName +
                                    " to target list because it is trusted for delegation.");
                        keepUser = true;
                    }

                    if (!keepUser && userAccFlags.HasFlag(UserAccountControlFlags
                        .TrustedToAuthenticateForDelegation))
                    {
                        Mq.Trace("Adding " + userName +
                                    " to target list because it is trusted for delegation.");
                        keepUser = true;
                    }

                    // Included patterns
                    if (!keepUser)
                    { 
                        foreach (string str in MyOptions.DomainUserMatchStrings)
                        {
                            if (userName.ToLower().Contains(str.ToLower()))                            
                            {
                                Mq.Trace("Adding " + userName +
                                            " to target list because it contained " + str + ".");
                                keepUser = true;
                                break;
                            }
                        }
                    }


                    // Finished testing
                    if(!keepUser)
                    {
                        continue;
                    }

                    // Must have matched something
                    // For common/frequent names,  force fully-qualified strict formats
                    if (MyOptions.DomainUserStrictStrings.Contains(userName, StringComparer.OrdinalIgnoreCase))
                    {
                        Mq.Trace("Using strict formats for " + userName + ".");

                        domainUsers.Add(String.Format(@"{0}\{1}", _targetDomainNetBIOSName, userName));
                        
                        if (!string.IsNullOrEmpty(resEnt.GetProperty("userPrincipalName")))
                        {
                            domainUsers.Add(resEnt.GetProperty("userPrincipalName"));
                        }

                        continue;
                    }
    
                    // Otherwise, go with the format preference from the config file
                    foreach (DomainUserNamesFormat dnuf in MyOptions.DomainUserNameFormats)
                    {
                        switch (dnuf)
                        {
                            case DomainUserNamesFormat.NetBIOS:
                                domainUsers.Add(String.Format(@"{0}\{1}",_targetDomainNetBIOSName,userName));
                                break;
                            case DomainUserNamesFormat.UPN:
                                if(!string.IsNullOrEmpty(resEnt.GetProperty("userPrincipalName")))
                                {
                                    domainUsers.Add(resEnt.GetProperty("userPrincipalName"));
                                }
                                else
                                {
                                    Mq.Trace("Adding " + userName + " with simple sAMAccountName because UPN is missing.");
                                    domainUsers.Add(userName);
                                }
                                break;
                            case DomainUserNamesFormat.sAMAccountName:
                                domainUsers.Add(userName);
                                break;
                        }
                    }                    
                }
                catch (Exception e)
                {
                    Mq.Trace(e.ToString());
                    continue;
                }                    
            }

            this._domainUsers = domainUsers;
        }
    }


    [Flags]
    public enum UserAccountControlFlags
    {
        Script = 0x1,
        AccountDisabled = 0x2,
        HomeDirectoryRequired = 0x8,
        AccountLockedOut = 0x10,
        PasswordNotRequired = 0x20,
        PasswordCannotChange = 0x40,
        EncryptedTextPasswordAllowed = 0x80,
        TempDuplicateAccount = 0x100,
        NormalAccount = 0x200,
        InterDomainTrustAccount = 0x800,
        WorkstationTrustAccount = 0x1000,
        ServerTrustAccount = 0x2000,
        PasswordDoesNotExpire = 0x10000,
        MnsLogonAccount = 0x20000,
        SmartCardRequired = 0x40000,
        TrustedForDelegation = 0x80000,
        AccountNotDelegated = 0x100000,
        UseDesKeyOnly = 0x200000,
        DontRequirePreauth = 0x400000,
        PasswordExpired = 0x800000,
        TrustedToAuthenticateForDelegation = 0x1000000,
        NoAuthDataRequired = 0x2000000
    }
}
