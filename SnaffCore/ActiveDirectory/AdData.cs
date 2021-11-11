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
        private Domain _currentDomain;
        private string _domainName;
        private string _targetDomain;
        private string _targetDc;
        private DirectorySearch _directorySearch;
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

        public string GetDomainName()
        {
            return _domainName;
        }

        public List<string> GetDfsNamespacePaths()
        {
            return _dfsNamespacePaths;
        }

        public DirectoryContext DirectoryContext { get; set; }
        private List<string> DomainControllers { get; set; } = new List<string>();

        public AdData()
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

            _directorySearch = new DirectorySearch(_targetDomain, _targetDc);

            SetDomainUsersAndComputers();
        }


        private void SetDomainUsersAndComputers()
        {
           
            List<string> domainComputers = new List<string>();
            List<string> domainUsers = new List<string>();

            try
            {
                Mq.Degub("Starting DFS Enumeration.");

                
                DfsFinder dfsFinder = new DfsFinder();
                List<DFSShare> dfsShares = dfsFinder.FindDfsShares(_directorySearch);
                _dfsNamespacePaths = new List<string>();
                foreach (DFSShare dfsShare in dfsShares)
                {
                    string dfsShareNamespacePath = @"\\" + _domainName + @"\" + dfsShare.DFSNamespace;
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
                    string ldapFilter = "(objectClass=computer)";

                    IEnumerable<SearchResultEntry> searchResultEntries = _directorySearch.QueryLdap(ldapFilter, ldapProperties, System.DirectoryServices.Protocols.SearchScope.Subtree);

                    int count = searchResultEntries.Count();



                    foreach (SearchResultEntry resEnt in searchResultEntries)
                    {
                        if (!String.IsNullOrEmpty(resEnt.GetProperty("dNSHostName")))
                        {
                            string computerName = resEnt.GetProperty("dNSHostName");
                            domainComputers.Add(computerName);
                        }
                    }

                    // now users
                    if (MyOptions.DomainUserRules)
                    {
                        ldapProperties = new string[] { "name", "adminCount", "sAMAccountName", "userAccountControl" };
                        ldapFilter = "(objectClass=user)";

                        searchResultEntries = _directorySearch.QueryLdap(ldapFilter, ldapProperties, System.DirectoryServices.Protocols.SearchScope.Subtree);
                        count = searchResultEntries.Count();

                        foreach (SearchResultEntry resEnt in searchResultEntries)
                        {
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

                                // skip computer accounts
                                if (userName.EndsWith("$"))
                                {
                                    continue;
                                }

                                //skip mailboxy accounts - domains always have a billion of these.
                                if (userName.IndexOf("mailbox", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    continue;
                                }

                                if (userName.IndexOf("mbx", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    continue;
                                }

                                // if it's got adminCount, keep it
                                if (resEnt.GetProperty("adminCount") == "1")
                                {
                                    Mq.Trace("Adding " + userName +
                                             " to target list because it had adminCount=1.");
                                    domainUsers.Add(userName);
                                    continue;
                                }

                                // if the password doesn't expire it's probably a service account
                                if (userAccFlags.HasFlag(UserAccountControlFlags.PasswordDoesNotExpire))
                                {
                                    Mq.Trace("Adding " + userName +
                                             " to target list because I think it's a service account.");
                                    domainUsers.Add(userName);
                                    continue;
                                }

                                if (userAccFlags.HasFlag(UserAccountControlFlags.DontRequirePreauth))
                                {
                                    Mq.Trace("Adding " + userName +
                                             " to target list because I think it's a service account.");
                                    domainUsers.Add(userName);
                                    continue;
                                }

                                if (userAccFlags.HasFlag(UserAccountControlFlags.TrustedForDelegation))
                                {
                                    Mq.Trace("Adding " + userName +
                                             " to target list because I think it's a service account.");
                                    domainUsers.Add(userName);
                                    continue;
                                }

                                if (userAccFlags.HasFlag(UserAccountControlFlags
                                    .TrustedToAuthenticateForDelegation))
                                {
                                    Mq.Trace("Adding " + userName +
                                             " to target list because I think it's a service account.");
                                    domainUsers.Add(userName);
                                    continue;
                                }

                                // if it matches a string we like, keep it
                                foreach (string str in MyOptions.DomainUserMatchStrings)
                                {
                                    if (userName.ToLower().Contains(str.ToLower()))
                                    {
                                        Mq.Trace("Adding " + userName +
                                                 " to target list because it contained " + str + ".");
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
                    }
                    this._domainComputers = domainComputers;
                    this._domainUsers = domainUsers;
                }
            }
            catch (Exception e)
            {
                Mq.Trace(e.ToString());
                throw;
            }
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
