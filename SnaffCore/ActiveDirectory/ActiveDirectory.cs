using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using SnaffCore.Concurrency;
using static SnaffCore.Config.Options;

namespace SnaffCore.ActiveDirectory
{
    public class AdData
    {
        private List<string> _domainComputers;
        private List<string> _domainUsers;
        private BlockingMq Mq { get; set; }

        public List<string> GetDomainComputers()
        {
            return _domainComputers;
        }

        public List<string> GetDomainUsers()
        {
            return _domainUsers;
        }

        private DirectoryContext DirectoryContext { get; set; }
        private List<string> DomainControllers { get; set; } = new List<string>();

        public AdData()
        {
            Mq = BlockingMq.GetMq();

            // figure out domain context
            if (MyOptions.TargetDomain == null && MyOptions.TargetDc == null)
            {
                try
                {
                    DirectoryContext =
                        new DirectoryContext(DirectoryContextType.Domain, Domain.GetCurrentDomain().Name);
                }
                catch (Exception e)
                {
                    Mq.Error(
                        "Problem figuring out DirectoryContext, you might need to define manually with -d and/or -c.");
                    Mq.Degub(e.ToString());
                    Mq.Terminate();
                }
            }
            else if (!String.IsNullOrEmpty(MyOptions.TargetDc))
            {
                DirectoryContext = new DirectoryContext(DirectoryContextType.Domain, MyOptions.TargetDc);
            }
            else if (!String.IsNullOrEmpty(MyOptions.TargetDomain))
            {
                DirectoryContext = new DirectoryContext(DirectoryContextType.Domain, MyOptions.TargetDomain);
            }
            SetDomainUsersAndComputers();
        }

        private void GetDomainControllers()
        {
            try
            {
                var dcCollection = DomainController.FindAll(DirectoryContext);
                foreach (DomainController dc in dcCollection)
                {
                    DomainControllers.Add(dc.IPAddress);
                }
            }
            catch (Exception e)
            {
                Mq.Error(
                    "Something went wrong trying to find domain controllers. Try defining manually with -c?");
                Mq.Degub(e.ToString());
                Mq.Terminate();
            }
        }

        private void SetDomainUsersAndComputers()
        {
            if (!String.IsNullOrEmpty(MyOptions.TargetDc))
            {
                DomainControllers.Add(MyOptions.TargetDc);
            }
            else
            {
                GetDomainControllers();
            }

            var domainComputers = new List<string>();
            var domainUsers = new List<string>();
            // we do this so if the first one fails we keep trying til we find a DC we can talk to.
            foreach (var domainController in DomainControllers)
            {
                try
                {
                    // TODO add support for user defined creds here.

                    using (var entry = new DirectoryEntry("LDAP://" + domainController))
                    {
                        using (var mySearcher = new DirectorySearcher(entry))
                        {
                            mySearcher.Filter = ("(objectClass=computer)");

                            // No size limit, reads all objects
                            mySearcher.SizeLimit = 0;

                            // Read data in pages of 250 objects. Make sure this value is below the limit configured in your AD domain (if there is a limit)
                            mySearcher.PageSize = 250;

                            // Let searcher know which properties are going to be used, and only load those
                            mySearcher.PropertiesToLoad.Add("name");
                            mySearcher.PropertiesToLoad.Add("dNSHostName");
                            mySearcher.PropertiesToLoad.Add("lastLogonTimeStamp");

                            foreach (SearchResult resEnt in mySearcher.FindAll())
                            {
                                // Note: Properties can contain multiple values.
                                if (resEnt.Properties["dNSHostName"].Count > 0)
                                {
                                    var computerName = (string) resEnt.Properties["dNSHostName"][0];
                                    domainComputers.Add(computerName);
                                }
                            }
                        }

                        this._domainComputers = domainComputers;

                        if (MyOptions.DomainUserRules)
                        {
                            // now users
                            using (var mySearcher = new DirectorySearcher(entry))
                            {
                                mySearcher.Filter = ("(objectClass=user)");

                                // No size limit, reads all objects
                                mySearcher.SizeLimit = 0;

                                // Read data in pages of 250 objects. Make sure this value is below the limit configured in your AD domain (if there is a limit)
                                mySearcher.PageSize = 250;

                                // Let searcher know which properties are going to be used, and only load those
                                mySearcher.PropertiesToLoad.Add("name");
                                mySearcher.PropertiesToLoad.Add("adminCount");
                                mySearcher.PropertiesToLoad.Add("sAMAccountName");
                                mySearcher.PropertiesToLoad.Add("userAccountControl");

                                foreach (SearchResult resEnt in mySearcher.FindAll())
                                {
                                    try
                                    {
                                        //busted account name
                                        if (resEnt.Properties["sAMAccountName"].Count == 0)
                                        {
                                            continue;
                                        }

                                        int uacFlags;
                                        bool succes =
                                            int.TryParse(resEnt.Properties["userAccountControl"][0].ToString(),
                                                out uacFlags);
                                        UserAccountControlFlags userAccFlags = (UserAccountControlFlags) uacFlags;

                                        if (userAccFlags.HasFlag(UserAccountControlFlags.AccountDisabled))
                                        {
                                            continue;
                                        }

                                        var userName = (string) resEnt.Properties["sAMAccountName"][0];

                                        // skip computer accounts
                                        if (userName.EndsWith("$"))
                                        {
                                            continue;
                                        }

                                        if (userName.IndexOf("mailbox", StringComparison.OrdinalIgnoreCase) >= 0)
                                        {
                                            continue;
                                        }

                                        if (userName.IndexOf("mbx", StringComparison.OrdinalIgnoreCase) >= 0)
                                        {
                                            continue;
                                        }

                                        // if it's got adminCount, keep it
                                        if (resEnt.Properties["adminCount"].Count != 0)
                                        {
                                            if (resEnt.Properties["adminCount"][0].ToString() == "1")
                                            {
                                                Mq.Trace("Adding " + userName +
                                                         " to target list because it had adminCount=1.");
                                                domainUsers.Add(userName);
                                                continue;
                                            }
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
                        }
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