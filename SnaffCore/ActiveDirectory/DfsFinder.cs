using System;
using System.Collections.Generic;
using System.Linq;
using SnaffCore.Concurrency;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Xml;
using SnaffCore.ActiveDirectory.LDAP;

namespace SnaffCore.ActiveDirectory
{
    public class DFSShare
    {
        public string Name { get; set; }
        public string RemoteServerName { get; set; }
        public string DFSNamespace { get; set; }
    }

    class DfsFinder
    {
        public BlockingMq Mq { get; set; }

        public DfsFinder()
        {
            Mq = BlockingMq.GetMq();
        }

        public List<DFSShare> FindDfsShares(DirectorySearch domainSearcher)
        {
            List<DFSShare> dfsShares = Get_DomainDFSShare(domainSearcher);
            return dfsShares;
        }

        private List<DFSShare> Get_DomainDFSShareV1(DirectorySearch _directorySearch)
        {
                var DFSShares = new List<DFSShare>();
                string[] properties = new string[] { "remoteservername", "pkt", "cn", "name" };
                string filter = @"(&(objectClass=fTDfs))";

            try
            {
                IEnumerable<SearchResultEntry> searchResultEntries = _directorySearch.QueryLdap(filter, properties, System.DirectoryServices.Protocols.SearchScope.Subtree);
                int count = searchResultEntries.Count();

                foreach (SearchResultEntry resEnt in searchResultEntries)
                {
                    string dfsnamespace = resEnt.DistinguishedName.Replace("CN=", "").Split(',')[0];

                    string[] RemoteNames = resEnt.GetPropertyAsArray(@"remoteservername");
                    byte[][] pkt = resEnt.GetPropertyAsArrayOfBytes("pkt");

                    // Add the namespace-level shares
                    if (RemoteNames != null)
                    {
                        foreach (string name in RemoteNames)
                        {
                            try
                            {
                                if (name.Contains(@"\"))
                                {

                                    DFSShares.Add(new DFSShare
                                    {
                                        Name = resEnt.GetProperty("name"),
                                        RemoteServerName = name.Split(new char[] { '\\' })[2],
                                        DFSNamespace = dfsnamespace
                                    });
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error parsing DFSv1 share : " + e);
                            }
                        }
                    }

                    // add the multi-level stuff
                    if (pkt != null && pkt[0] != null)
                    {
                        List<DFSShare> shares = Parse_Pkt(pkt[0] as byte[]);
                        if (shares != null)
                        {
                            foreach (var share in shares)
                            {
                                DFSShares.Add(share);
                            }
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Get-DomainDFSShareV1 error : " + e);
            }
            
            return DFSShares;
        }

        private List<DFSShare> Get_DomainDFSShareV2(DirectorySearch _directorySearch)
        {
            var DFSShares = new List<DFSShare>();
            string[] properties = new string[] { @"msdfs-linkpathv2", @"msDFS-TargetListv2", "cn", "name" };
            string filter = @"(&(objectClass=msDFS-Linkv2))";

            try
            {
                IEnumerable<SearchResultEntry> searchResultEntries = _directorySearch.QueryLdap(filter, properties, System.DirectoryServices.Protocols.SearchScope.Subtree);
                int count = searchResultEntries.Count();

                foreach (SearchResultEntry resEnt in searchResultEntries)
                {
                    //Console.WriteLine("Found a DFSv2 entry.");
                    string dfsnamespace = resEnt.DistinguishedName.Replace("CN=", "").Split(',')[1];

                    var target_list = resEnt.GetPropertyAsBytes(@"msdfs-targetlistv2");
                    var xml = new XmlDocument();
                    string thing = System.Text.Encoding.Unicode.GetString(target_list.Skip(2).Take(target_list.Length - 1 + 1 - 2).ToArray());
                    xml.LoadXml(System.Text.Encoding.Unicode.GetString(target_list.Skip(2).Take(target_list.Length - 1 + 1 - 2).ToArray()));

                    if (xml.FirstChild != null)
                    {
                        foreach (XmlNode node in xml.ChildNodes)
                        {
                            foreach (XmlNode babbynode in node.ChildNodes)
                            {
                                try
                                {
                                    var Target = node.InnerText;
                                    if (Target.Contains(@"\"))
                                    {
                                        var DFSroot = Target.Split('\\')[3];
                                        string ShareName = resEnt.GetProperty(@"msdfs-linkpathv2").Replace("/","\\");
                                        DFSShares.Add(new DFSShare { Name = $@"{DFSroot}{ShareName}", RemoteServerName = Target.Split('\\')[2], DFSNamespace = dfsnamespace });
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Error in parsing DFSv2 share : " + e);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Get-DomainDfsShareV2 error : " + e);
            }
            return DFSShares;
        }

        public List<DFSShare> Get_DomainDFSShare(DirectorySearch DFSSearcher)
        {
            var DFSShares = new List<DFSShare>();

            DFSShares.AddRange(Get_DomainDFSShareV1(DFSSearcher));
            DFSShares.AddRange(Get_DomainDFSShareV2(DFSSearcher));

            return DFSShares;
        }

        private static List<DFSShare> Parse_Pkt(byte[] Pkt)
        {
            var bin = Pkt;
            var blob_version = BitConverter.ToUInt32(bin.Skip(0).Take(4).ToArray(), 0);
            var blob_element_count = BitConverter.ToUInt32(bin.Skip(4).Take(4).ToArray(), 0);
            int blob_data_end = 0;
            var offset = 8;
            string prefix = null;
            string blob_name = null;
            List<string> target_list = null;
            var shares = new List<DFSShare>();
            // https://msdn.microsoft.com/en-us/library/cc227147.aspx
            var object_list = new List<Dictionary<string, object>>();

            for (var i = 1; i <= blob_element_count; i++)
            {
                var blob_name_size_start = offset;
                var blob_name_size_end = offset + 1;
                var blob_name_size = BitConverter.ToUInt16(bin.Skip(blob_name_size_start).Take(blob_name_size_end + 1 - blob_name_size_start).ToArray(), 0);

                var blob_name_start = blob_name_size_end + 1;
                var blob_name_end = blob_name_start + blob_name_size - 1;
                blob_name = System.Text.Encoding.Unicode.GetString(bin.Skip(blob_name_start).Take(blob_name_end + 1 - blob_name_start).ToArray());

                var blob_data_size_start = blob_name_end + 1;
                var blob_data_size_end = blob_data_size_start + 3;
                var blob_data_size = BitConverter.ToUInt32(bin.Skip(blob_data_size_start).Take(blob_data_size_end + 1 - blob_data_size_start).ToArray(), 0);

                var blob_data_start = blob_data_size_end + 1;
                blob_data_end = (int)(blob_data_start + blob_data_size - 1);
                var blob_data = bin.Skip(blob_data_start).Take(blob_data_end + 1 - blob_data_start);
                if (blob_name == @"\siteroot") { }
                else if (blob_name.StartsWith(@"\domainroot"))
                {
                    // Parse DFSNamespaceRootOrLinkBlob object. Starts with variable length DFSRootOrLinkIDBlob which we parse first...
                    // DFSRootOrLinkIDBlob
                    var root_or_link_guid_start = 0;
                    var root_or_link_guid_end = 15;
                    var root_or_link_guid = blob_data.Skip(root_or_link_guid_start).Take(root_or_link_guid_end + 1 - root_or_link_guid_start);
                    var guid = new Guid(root_or_link_guid.ToArray()); // should match $guid_str
                    var prefix_size_start = root_or_link_guid_end + 1;
                    var prefix_size_end = prefix_size_start + 1;
                    var prefix_size = BitConverter.ToUInt16(blob_data.Skip(prefix_size_start).Take(prefix_size_end + 1 - prefix_size_start).ToArray(), 0);
                    var prefix_start = prefix_size_end + 1;
                    var prefix_end = prefix_start + prefix_size - 1;
                    prefix = System.Text.Encoding.Unicode.GetString(blob_data.Skip(prefix_start).Take(prefix_end + 1 - prefix_start).ToArray());

                    var short_prefix_size_start = prefix_end + 1;
                    var short_prefix_size_end = short_prefix_size_start + 1;
                    var short_prefix_size = BitConverter.ToUInt16(blob_data.Skip(short_prefix_size_start).Take(short_prefix_size_end + 1 - short_prefix_size_start).ToArray(), 0);
                    var short_prefix_start = short_prefix_size_end + 1;
                    var short_prefix_end = short_prefix_start + short_prefix_size - 1;
                    var short_prefix = System.Text.Encoding.Unicode.GetString(blob_data.Skip(short_prefix_start).Take(short_prefix_end + 1 - short_prefix_start).ToArray());

                    var type_start = short_prefix_end + 1;
                    var type_end = type_start + 3;
                    var type = BitConverter.ToUInt32(blob_data.Skip(type_start).Take(type_end + 1 - type_start).ToArray(), 0);

                    var state_start = type_end + 1;
                    var state_end = state_start + 3;
                    var state = BitConverter.ToUInt32(blob_data.Skip(state_start).Take(state_end + 1 - state_start).ToArray(), 0);

                    var comment_size_start = state_end + 1;
                    var comment_size_end = comment_size_start + 1;
                    var comment_size = BitConverter.ToUInt16(blob_data.Skip(comment_size_start).Take(comment_size_end + 1 - comment_size_start).ToArray(), 0);
                    var comment_start = comment_size_end + 1;
                    var comment_end = comment_start + comment_size - 1;
                    var comment = "";
                    if (comment_size >= 0)
                    {
                        comment = System.Text.Encoding.Unicode.GetString(blob_data.Skip(comment_start).Take(comment_end + 1 - comment_start).ToArray());
                    }
                    var prefix_timestamp_start = comment_end + 1;
                    var prefix_timestamp_end = prefix_timestamp_start + 7;
                    // https://msdn.microsoft.com/en-us/library/cc230324.aspx FILETIME
                    var prefix_timestamp = blob_data.Skip(prefix_timestamp_start).Take(prefix_timestamp_end + 1 - prefix_timestamp_start); // dword lowDateTime #dword highdatetime
                    var state_timestamp_start = prefix_timestamp_end + 1;
                    var state_timestamp_end = state_timestamp_start + 7;
                    var state_timestamp = blob_data.Skip(state_timestamp_start).Take(state_timestamp_end + 1 - state_timestamp_start);
                    var comment_timestamp_start = state_timestamp_end + 1;
                    var comment_timestamp_end = comment_timestamp_start + 7;
                    var comment_timestamp = blob_data.Skip(comment_timestamp_start).Take(comment_timestamp_end + 1 - comment_timestamp_start);
                    var version_start = comment_timestamp_end + 1;
                    var version_end = version_start + 3;
                    var version = BitConverter.ToUInt32(blob_data.Skip(version_start).Take(version_end + 1 - version_start).ToArray(), 0);

                    // Parse rest of DFSNamespaceRootOrLinkBlob here
                    var dfs_targetlist_blob_size_start = version_end + 1;
                    var dfs_targetlist_blob_size_end = dfs_targetlist_blob_size_start + 3;
                    var dfs_targetlist_blob_size = BitConverter.ToUInt32(blob_data.Skip(dfs_targetlist_blob_size_start).Take(dfs_targetlist_blob_size_end + 1 - dfs_targetlist_blob_size_start).ToArray(), 0);

                    var dfs_targetlist_blob_start = dfs_targetlist_blob_size_end + 1;
                    var dfs_targetlist_blob_end = (int)(dfs_targetlist_blob_start + dfs_targetlist_blob_size - 1);
                    var dfs_targetlist_blob = blob_data.Skip(dfs_targetlist_blob_start).Take(dfs_targetlist_blob_end + 1 - dfs_targetlist_blob_start);
                    var reserved_blob_size_start = dfs_targetlist_blob_end + 1;
                    var reserved_blob_size_end = reserved_blob_size_start + 3;
                    var reserved_blob_size = BitConverter.ToUInt32(blob_data.Skip(reserved_blob_size_start).Take(reserved_blob_size_end + 1 - reserved_blob_size_start).ToArray(), 0);

                    var reserved_blob_start = reserved_blob_size_end + 1;
                    var reserved_blob_end = (int)(reserved_blob_start + reserved_blob_size - 1);
                    var reserved_blob = blob_data.Skip(reserved_blob_start).Take(reserved_blob_end + 1 - reserved_blob_start);
                    var referral_ttl_start = reserved_blob_end + 1;
                    var referral_ttl_end = referral_ttl_start + 3;
                    var referral_ttl = BitConverter.ToUInt32(blob_data.Skip(referral_ttl_start).Take(referral_ttl_end + 1 - referral_ttl_start).ToArray(), 0);

                    // Parse DFSTargetListBlob
                    var target_count_start = 0;
                    var target_count_end = target_count_start + 3;
                    var target_count = BitConverter.ToUInt32(dfs_targetlist_blob.Skip(target_count_start).Take(target_count_end + 1 - target_count_start).ToArray(), 0);
                    var t_offset = target_count_end + 1;

                    for (var j = 1; j <= target_count; j++)
                    {
                        var target_entry_size_start = t_offset;
                        var target_entry_size_end = target_entry_size_start + 3;
                        var target_entry_size = BitConverter.ToUInt32(dfs_targetlist_blob.Skip(target_entry_size_start).Take(target_entry_size_end + 1 - target_entry_size_start).ToArray(), 0);
                        var target_time_stamp_start = target_entry_size_end + 1;
                        var target_time_stamp_end = target_time_stamp_start + 7;
                        // FILETIME again or special if priority rank and priority class 0
                        var target_time_stamp = dfs_targetlist_blob.Skip(target_time_stamp_start).Take(target_time_stamp_end + 1 - target_time_stamp_start);
                        var target_state_start = target_time_stamp_end + 1;
                        var target_state_end = target_state_start + 3;
                        var target_state = BitConverter.ToUInt32(dfs_targetlist_blob.Skip(target_state_start).Take(target_state_end + 1 - target_state_start).ToArray(), 0);

                        var target_type_start = target_state_end + 1;
                        var target_type_end = target_type_start + 3;
                        var target_type = BitConverter.ToUInt32(dfs_targetlist_blob.Skip(target_type_start).Take(target_type_end + 1 - target_type_start).ToArray(), 0);

                        var server_name_size_start = target_type_end + 1;
                        var server_name_size_end = server_name_size_start + 1;
                        var server_name_size = BitConverter.ToUInt16(dfs_targetlist_blob.Skip(server_name_size_start).Take(server_name_size_end + 1 - server_name_size_start).ToArray(), 0);

                        var server_name_start = server_name_size_end + 1;
                        var server_name_end = server_name_start + server_name_size - 1;
                        var server_name = System.Text.Encoding.Unicode.GetString(dfs_targetlist_blob.Skip(server_name_start).Take(server_name_end + 1 - server_name_start).ToArray());

                        var share_name_size_start = server_name_end + 1;
                        var share_name_size_end = share_name_size_start + 1;
                        var share_name_size = BitConverter.ToUInt16(dfs_targetlist_blob.Skip(share_name_size_start).Take(share_name_size_end + 1 - share_name_size_start).ToArray(), 0);
                        var share_name_start = share_name_size_end + 1;
                        var share_name_end = share_name_start + share_name_size - 1;
                        var share_name = System.Text.Encoding.Unicode.GetString(dfs_targetlist_blob.Skip(share_name_start).Take(share_name_end + 1 - share_name_start).ToArray());

                        if (target_list == null)
                            target_list = new List<string>();
                        target_list.Add($@"\\{server_name}\{share_name}");
                        t_offset = share_name_end + 1;
                    }
                }
                offset = blob_data_end + 1;
                var dfs_pkt_properties = new Dictionary<string, object>
                {
                    { @"Name", blob_name },
                    { @"Prefix", prefix },
                    { @"TargetList", target_list }
                };
                object_list.Add(dfs_pkt_properties);
                prefix = null;
                blob_name = null;
                target_list = null;
            }

            if (object_list != null)
            {
                foreach (var item in object_list)
                {
                    if(item[@"Prefix"] == null)
                    {
                        continue;
                    }

                    prefix = item[@"Prefix"] as string;
                    string dfsns = prefix.Split(new char[] {'\\'},3)[2];

                    List<string> targetList = item[@"TargetList"] as List<string>;
                    if (targetList != null)
                    {
                        foreach (var target in targetList)
                        {
                            //shareDict.Add();
                            var share = new DFSShare();

                            string[] target_parts = target.Split(new char[] { '\\' });
                            share.DFSNamespace = dfsns;
                            share.Name = target_parts[3];
                            share.RemoteServerName = target_parts[2];

                            shares.Add(share);
                        }
                    }
                }
            }

            return shares;
        }
    }
}
