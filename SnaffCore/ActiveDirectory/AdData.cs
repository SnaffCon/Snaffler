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
    public sealed class AdData
    {
        private List<string> _domainComputers = new List<string>();
        private List<string> _domainUsers = new List<string>();
        private Dictionary<string, string> _dfsSharesDict;
        private List<string> _dfsNamespacePaths;
        private Domain _currentDomain;
        private string _domainName;
        private string _targetDomain;
        private string _targetDc;
        private string _targetDomainNetBIOSName;
        private DirectorySearch _directorySearch;
        private BlockingMq Mq { get; set; }

        private static readonly Lazy<AdData> lazy =
            new Lazy<AdData>(() => new AdData());
        public static AdData AdDataInstance
        {
            get { return lazy.Value; }
        }

        private AdData()
        {

        }

        public List<string> GetDomainComputers()
        {
            return _domainComputers;
        }

        public List<string> GetDomainUsers()
        {
            return _domainUsers;
        }

        public Dictionary<string, string> GetDfsSharesDict()
        {
            return _dfsSharesDict;
        }

        public List<string> GetDfsNamespacePaths()
        {
            return _dfsNamespacePaths;
        }

        public DirectorySearch GetDirectorySearch()
        {
            if (_directorySearch == null)
            {
                SetDirectorySearch();
            }
            return _directorySearch;
        }

        public DirectoryContext DirectoryContext { get; set; }
        private List<string> DomainControllers { get; set; } = new List<string>();

        private string GetNetBiosDomainName()
        {
            string ldapBase = $"CN=Partitions,CN=Configuration,DC={_targetDomain.Replace(".", ",DC=")}";

            DirectorySearch ds = new DirectorySearch(_targetDomain, _targetDc, ldapBase, null, null, 0, false);

            string[] ldapProperties = new string[] { "netbiosname"};
            string ldapFilter = string.Format("(&(objectcategory=Crossref)(dnsRoot={0})(netBIOSName=*))",_targetDomain);

            foreach (SearchResultEntry sre in ds.QueryLdap(ldapFilter, ldapProperties, System.DirectoryServices.Protocols.SearchScope.Subtree))
            {
                return sre.GetProperty("netbiosname");
            }

            return null;
        }


        private void SetDirectorySearch()
        {
            Mq = BlockingMq.GetMq();

            // target domain and dc set
            if ((!string.IsNullOrEmpty(MyOptions.TargetDomain)) && (!string.IsNullOrEmpty(MyOptions.TargetDc)))
            {
                Mq.Trace("Target DC and Domain specified: " + MyOptions.TargetDomain + " + " + MyOptions.TargetDc);
                _targetDc = MyOptions.TargetDc;
                _targetDomain = MyOptions.TargetDomain;
            }
            // no target DC or domain set
            else
            {
                Mq.Trace("Getting current domain from user context.");
                _currentDomain = Domain.GetCurrentDomain();
                _targetDomain = _currentDomain.Name;
                _targetDc = _targetDomain;
            }

            _targetDomainNetBIOSName = GetNetBiosDomainName();
            DirectorySearch directorySearch = new DirectorySearch(_targetDomain, _targetDc);
            _directorySearch = directorySearch;
        }

        public void SetDfsPaths()
        {
            DirectorySearch ds = GetDirectorySearch();

            try
            {
                Mq.Degub("Starting DFS Enumeration.");

                DfsFinder dfsFinder = new DfsFinder();
                List<DFSShare> dfsShares = dfsFinder.FindDfsShares(ds);

                _dfsSharesDict = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                _dfsNamespacePaths = new List<string>();
                string realPath;

                foreach (DFSShare dfsShare in dfsShares)
                {
                    // Construct the UNC path to this DFS share and add it to the list.  
                    // We use this structure as a to-do list in the ShareFinder code, skipping DFS shares that have already been processed
                    string dfsShareNamespacePath = @"\\" + _targetDomain + @"\" + dfsShare.DFSFolderPath;
                    List<string> hostnames = new List<string>();

                    if (!_dfsNamespacePaths.Contains(dfsShareNamespacePath))
                    {
                        _dfsNamespacePaths.Add(dfsShareNamespacePath);
                    }

                    // Calculate a long and a short name version for each "real" share path in lowercase.  Admins can set either in AD and
                    //    we may get either for our scan (depending on how we got our computer list.
                    // This simplifies the cross-referencing of actual server shares back to DFS paths in the ShareFinder code.

                    hostnames.Add(dfsShare.RemoteServerName);

                    if (dfsShare.RemoteServerName.EndsWith(_targetDomain, StringComparison.OrdinalIgnoreCase))
                    {   // share path has FQDN so crack out the short hostname
                        hostnames.Add(dfsShare.RemoteServerName.Split('.')[0]);
                    }
                    else
                    {   // share path has short name so append domain for FQDN
                        hostnames.Add(String.Format("{0}.{1}", dfsShare.RemoteServerName, _targetDomain));
                    }

                    // Add these paths as keys in the dictionary
                    foreach (string h in hostnames)
                    {
                        realPath = String.Format(@"\\{0}\{1}", h, dfsShare.RemoteShareName);

                        if (!_dfsSharesDict.ContainsKey(realPath))
                        {
                            _dfsSharesDict.Add(realPath, dfsShareNamespacePath);
                        }
                    }
                }
                
                Mq.Info("Found " + _dfsSharesDict.Count.ToString() + " DFS Shares in " + _dfsNamespacePaths.Count.ToString() + " namespaces.");

                Mq.Degub("Finished DFS Enumeration.");

            }
            catch (Exception e)
            {
                Mq.Trace(e.ToString());
            }
        }

        private static Random random = new Random();

        public void SetDomainComputers(string LdapFilter)
        {
            DirectorySearch ds = GetDirectorySearch();

            List<string> domainComputers = new List<string>();

            try
            {
                if (!MyOptions.DfsOnly)
                {
                    // if we aren't limiting the scan to DFS shares then let's get some computer targets.

                    List<string> ldapPropertiesList = new List<string> { "name", "dNSHostName", "lastLogonTimeStamp" };
                    string ldapFilter = LdapFilter;

                    // extremely dirty hack to break a sig I once saw for Snaffler's LDAP queries. ;-)
                    int num = random.Next(1, 5);
                    while (num > 0)
                    {
                        Guid guid = Guid.NewGuid();
                        ldapPropertiesList.Add(guid.ToString());
                        --num;
                    }
                    string[] ldapProperties = ldapPropertiesList.ToArray();

                    IEnumerable<SearchResultEntry> searchResultEntries = ds.QueryLdap(ldapFilter, ldapProperties, System.DirectoryServices.Protocols.SearchScope.Subtree);

                    // set a window of "the last 4 months" - if a computer hasn't logged in to the domain in 4 months it's probably gone.
                    DateTime validLltsWindow = DateTime.Now.AddMonths(-4);
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

                        try
                        {
                            // get the last logon timestamp value as a datetime
                            string lltsString = resEnt.GetProperty("lastlogontimestamp");
                            long lltsLong;
                            long.TryParse(lltsString, out lltsLong);
                            DateTime lltsDateTime = DateTime.FromFileTime(lltsLong);
                            // compare it to our window, and if lltsDateTime is older, skip the computer acct.
                            if (lltsDateTime <= validLltsWindow)
                            {
                                continue;
                            }
                        }
                        catch (Exception e)
                        {
                            Mq.Error("Error calculating lastLogonTimeStamp for computer account " + resEnt.DistinguishedName);
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
            DirectorySearch ds = GetDirectorySearch();
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
