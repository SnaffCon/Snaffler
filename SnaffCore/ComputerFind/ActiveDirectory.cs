using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;

namespace SnaffCore.ComputerFind
{
    public class ActiveDirectory
    {
        private List<string> _domainComputers;
        private Config.Config Config { get; set; }

        public List<string> DomainComputers =>
            // only compute this once
            _domainComputers
            ?? (_domainComputers = GetDomainComputers());

        private DirectoryContext DirectoryContext { get; set; }
        private List<string> DomainControllers { get; set; } = new List<string>();

        public ActiveDirectory(Config.Config config)
        {
            Config = config;

            // setup the necessary vars
            if (Config.Options.TargetDomain == null && Config.Options.TargetDc == null)
            {
                try
                {
                    DirectoryContext =
                        new DirectoryContext(DirectoryContextType.Domain, Domain.GetCurrentDomain().Name);
                }
                catch (Exception e)
                {
                    Config.Mq.Error(
                        "Problem figuring out DirectoryContext, you might need to define manually with -d and/or -c.");
                    Config.Mq.Degub(e.ToString());
                    Config.Mq.Terminate();
                }
            }
            else if (!String.IsNullOrEmpty(Config.Options.TargetDc))
            {
                DirectoryContext = new DirectoryContext(DirectoryContextType.Domain, Config.Options.TargetDc);
            }
            else if (!String.IsNullOrEmpty(Config.Options.TargetDomain))
            {
                DirectoryContext = new DirectoryContext(DirectoryContextType.Domain, Config.Options.TargetDomain);
            }
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
                Config.Mq.Error(
                    "Something went wrong trying to find domain controllers. Try defining manually with -c?");
                Config.Mq.Degub(e.ToString());
                Config.Mq.Terminate();
            }
        }


        private List<string> GetDomainComputers()
        {
            if (!String.IsNullOrEmpty(Config.Options.TargetDc))
            {
                DomainControllers.Add(Config.Options.TargetDc);
            }
            else
            {
                GetDomainControllers();
            }

            var domainComputers = new List<string>();
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
                                // TODO figure out how to compare timestamp
                                //if (resEnt.Properties["lastLogonTimeStamp"])
                                //{
                                //    continue;
                                //}
                                // Note: Properties can contain multiple values.
                                if (resEnt.Properties["dNSHostName"].Count > 0)
                                {
                                    var computerName = (string) resEnt.Properties["dNSHostName"][0];
                                    domainComputers.Add(computerName);
                                }
                            }
                        }
                    }

                    return domainComputers;
                }
                catch (Exception e)
                {
                    Config.Mq.Trace(e.ToString());
                    throw;
                }
            }

            return null;
        }
    }
}