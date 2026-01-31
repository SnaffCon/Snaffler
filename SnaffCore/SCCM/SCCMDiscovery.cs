using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using SnaffCore.Concurrency;

namespace SnaffCore.SCCM
{
    public class SCCMDiscovery
    {
        private BlockingMq Mq { get; set; }
        private readonly string _domain;
        private readonly string _username;
        private readonly string _password;
        private readonly bool _debug;
        private readonly int _ldapPort;

        public SCCMDiscovery(string domain, string username = null, string password = null,
            bool debug = false, int ldapPort = 389)
        {
            Mq = BlockingMq.GetMq();
            _domain = domain;
            _username = username;
            _password = password;
            _debug = debug;
            _ldapPort = ldapPort;
        }

        public List<SCCMServer> DiscoverSCCMServers()
        {
            var servers = new List<SCCMServer>();

            try
            {
                Mq.Info($"[SCCM Discovery] Querying domain: {_domain}");

                servers.AddRange(QueryViaSMS_SiteSystemServer());
                servers.AddRange(QueryViaServiceConnectionPoint());
                servers.AddRange(QueryViaComputerNamePatterns());

                var uniqueServers = servers
                    .GroupBy(s => s.Hostname.ToLower())
                    .Select(g => g.First())
                    .ToList();

                Mq.Info($"[SCCM Discovery] Found {uniqueServers.Count} unique SCCM servers");

                return uniqueServers;
            }
            catch (Exception ex)
            {
                Mq.Error($"[SCCM Discovery] Error: {ex.Message}");
                if (_debug)
                {
                    Mq.Degub($"Stack trace: {ex.StackTrace}");
                }
                return servers;
            }
        }

        private List<SCCMServer> QueryViaSMS_SiteSystemServer()
        {
            var servers = new List<SCCMServer>();

            try
            {
                var ldapPath = BuildLdapPath("CN=System Management,CN=System");

                using (var entry = new DirectoryEntry(ldapPath, _username, _password))
                using (var searcher = new DirectorySearcher(entry))
                {
                    searcher.Filter = "(objectClass=mSSMSSite)";
                    searcher.PropertiesToLoad.Add("mSSMSSiteCode");
                    searcher.PropertiesToLoad.Add("cn");
                    searcher.SearchScope = System.DirectoryServices.SearchScope.Subtree;

                    var results = searcher.FindAll();

                    if (_debug)
                    {
                        Mq.Degub($"[SCCM] Found {results.Count} SCCM site(s) via SMS query");
                    }

                    foreach (SearchResult result in results)
                    {
                        var siteCode = result.Properties["mSSMSSiteCode"]?[0]?.ToString();
                        var siteName = result.Properties["cn"]?[0]?.ToString();

                        if (!string.IsNullOrEmpty(siteCode))
                        {
                            servers.AddRange(FindDistributionPointsForSite(siteCode, siteName));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_debug)
                {
                    Mq.Degub($"[SCCM] SMS_SiteSystemServer query failed: {ex.Message}");
                }
            }

            return servers;
        }

        private List<SCCMServer> FindDistributionPointsForSite(string siteCode, string siteName)
        {
            var servers = new List<SCCMServer>();

            try
            {
                var ldapPath = BuildLdapPath("");

                using (var entry = new DirectoryEntry(ldapPath, _username, _password))
                using (var searcher = new DirectorySearcher(entry))
                {
                    searcher.Filter = "(&(objectClass=mSSMSSiteSystemServer)(mSSMSDefaultMP=*))";
                    searcher.PropertiesToLoad.Add("mSSMSSiteName");
                    searcher.PropertiesToLoad.Add("cn");
                    searcher.PropertiesToLoad.Add("dNSHostName");
                    searcher.PropertiesToLoad.Add("mSSMSRolesConfigured");
                    searcher.SearchScope = System.DirectoryServices.SearchScope.Subtree;

                    var results = searcher.FindAll();

                    foreach (SearchResult result in results)
                    {
                        var hostname = result.Properties["dNSHostName"]?[0]?.ToString();
                        var cn = result.Properties["cn"]?[0]?.ToString();
                        var roles = result.Properties["mSSMSRolesConfigured"];

                        if (string.IsNullOrEmpty(hostname) && !string.IsNullOrEmpty(cn))
                        {
                            hostname = cn.Split(',')[0].Replace("CN=", "");
                        }

                        if (!string.IsNullOrEmpty(hostname))
                        {
                            string role = "Distribution Point";
                            if (roles != null && roles.Count > 0)
                            {
                                var rolesList = new List<string>();
                                foreach (var r in roles)
                                {
                                    rolesList.Add(r.ToString());
                                }
                                role = string.Join(", ", rolesList);
                            }

                            servers.Add(new SCCMServer
                            {
                                Hostname = hostname,
                                SiteCode = siteCode,
                                SiteName = siteName,
                                Role = role,
                                DiscoveryMethod = "LDAP-SiteSystem"
                            });

                            if (_debug)
                            {
                                Mq.Degub($"[SCCM] Found DP: {hostname} (Site: {siteCode})");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_debug)
                {
                    Mq.Degub($"[SCCM] Distribution Point query failed: {ex.Message}");
                }
            }

            return servers;
        }

        private List<SCCMServer> QueryViaServiceConnectionPoint()
        {
            var servers = new List<SCCMServer>();

            try
            {
                var ldapPath = BuildLdapPath("");

                using (var entry = new DirectoryEntry(ldapPath, _username, _password))
                using (var searcher = new DirectorySearcher(entry))
                {
                    searcher.Filter = "(&(objectClass=serviceConnectionPoint)(cn=SMS-MP-*))";
                    searcher.PropertiesToLoad.Add("serviceBindingInformation");
                    searcher.PropertiesToLoad.Add("cn");
                    searcher.SearchScope = System.DirectoryServices.SearchScope.Subtree;

                    var results = searcher.FindAll();

                    if (_debug && results.Count > 0)
                    {
                        Mq.Degub($"[SCCM] Found {results.Count} service connection points");
                    }

                    foreach (SearchResult result in results)
                    {
                        var bindingInfo = result.Properties["serviceBindingInformation"];
                        if (bindingInfo != null && bindingInfo.Count > 0)
                        {
                            foreach (var info in bindingInfo)
                            {
                                var infoStr = info.ToString();
                                if (infoStr.Contains("://"))
                                {
                                    var hostname = ExtractHostnameFromUrl(infoStr);
                                    if (!string.IsNullOrEmpty(hostname))
                                    {
                                        servers.Add(new SCCMServer
                                        {
                                            Hostname = hostname,
                                            Role = "Management Point",
                                            DiscoveryMethod = "LDAP-ServiceConnectionPoint"
                                        });

                                        if (_debug)
                                        {
                                            Mq.Degub($"[SCCM] Found MP: {hostname}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_debug)
                {
                    Mq.Degub($"[SCCM] Service Connection Point query failed: {ex.Message}");
                }
            }

            return servers;
        }

        private List<SCCMServer> QueryViaComputerNamePatterns()
        {
            var servers = new List<SCCMServer>();

            try
            {
                var ldapPath = BuildLdapPath("");

                using (var entry = new DirectoryEntry(ldapPath, _username, _password))
                using (var searcher = new DirectorySearcher(entry))
                {
                    searcher.Filter = "(|(cn=*SCCM*)(cn=*SMS*)(description=*SCCM*)(description=*SMS*)(description=*Configuration Manager*))";
                    searcher.PropertiesToLoad.Add("dNSHostName");
                    searcher.PropertiesToLoad.Add("cn");
                    searcher.PropertiesToLoad.Add("description");
                    searcher.SearchScope = System.DirectoryServices.SearchScope.Subtree;

                    var results = searcher.FindAll();

                    if (_debug && results.Count > 0)
                    {
                        Mq.Degub($"[SCCM] Found {results.Count} potential servers by name pattern");
                    }

                    foreach (SearchResult result in results)
                    {
                        var hostname = result.Properties["dNSHostName"]?[0]?.ToString();
                        var description = result.Properties["description"]?[0]?.ToString();

                        if (!string.IsNullOrEmpty(hostname))
                        {
                            servers.Add(new SCCMServer
                            {
                                Hostname = hostname,
                                Role = "Potential SCCM Server",
                                Description = description,
                                DiscoveryMethod = "LDAP-NamePattern"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_debug)
                {
                    Mq.Degub($"[SCCM] Name pattern query failed: {ex.Message}");
                }
            }

            return servers;
        }

        private string BuildLdapPath(string additionalPath)
        {
            var dcComponents = _domain.Split('.')
                .Select(part => $"DC={part}");
            var dcString = string.Join(",", dcComponents);

            if (string.IsNullOrEmpty(additionalPath))
            {
                return $"LDAP://{dcString}";
            }

            return $"LDAP://{additionalPath},{dcString}";
        }


        private string ExtractHostnameFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.Host;
            }
            catch
            {
                var parts = url.Split(new[] { "://" }, StringSplitOptions.None);
                if (parts.Length >= 2)
                    return parts[1].Split(':')[0];
                return null;
            }
        }


        public List<SCCMServer> ValidateServers(List<SCCMServer> servers)
        {
            var validatedServers = new List<SCCMServer>();

            Mq.Info($"[SCCM] Validating {servers.Count} server(s)...");

            foreach (var server in servers)
            {
                try
                {
                    var addresses = Dns.GetHostAddresses(server.Hostname);
                    server.IPAddress = addresses.FirstOrDefault()?.ToString();

                    var sharePath = $"\\\\{server.Hostname}\\SCCMContentLib$";

                    if (System.IO.Directory.Exists(sharePath))
                    {
                        server.SCCMContentLibAccessible = true;
                        validatedServers.Add(server);

                        if (_debug)
                        {
                            Mq.Degub($"[SCCM] Validated: {server.Hostname}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (_debug)
                    {
                        Mq.Degub($"[SCCM] Validation failed for {server.Hostname}: {ex.Message}");
                    }
                }
            }

            Mq.Info($"[SCCM] {validatedServers.Count}/{servers.Count} server(s) validated");

            return validatedServers;
        }

        public List<string> BuildTargetPaths(List<SCCMServer> servers)
        {
            var targets = new List<string>();

            foreach (var server in servers)
            {
                targets.Add($"\\\\{server.Hostname}\\SCCMContentLib$");

                if (!string.IsNullOrEmpty(server.SiteCode))
                {
                    targets.Add($"\\\\{server.Hostname}\\SMS_{server.SiteCode}");
                    targets.Add($"\\\\{server.Hostname}\\SMSPKG{server.SiteCode}");
                }
            }

            return targets.Distinct().ToList();
        }
    }

    public class SCCMServer
    {
        public string Hostname { get; set; }
        public string IPAddress { get; set; }
        public string SiteCode { get; set; }
        public string SiteName { get; set; }
        public string Role { get; set; }
        public string Description { get; set; }
        public string DiscoveryMethod { get; set; }
        public bool SCCMContentLibAccessible { get; set; }

        public override string ToString()
        {
            return $"{Hostname} ({Role}) - Site: {SiteCode ?? "Unknown"}";
        }
    }
}
