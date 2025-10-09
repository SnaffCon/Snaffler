using SnaffCore.Classifiers;
using SnaffCore.Concurrency;
using SnaffCore.TreeWalk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SnaffCore.ActiveDirectory;
using SnaffCore.Classifiers.EffectiveAccess;
using static SnaffCore.Config.Options;
using SnaffCore.SCCM;


namespace SnaffCore.ShareFind
{
    public class ShareFinder
    {
        private BlockingMq Mq { get; set; }
        private BlockingStaticTaskScheduler TreeTaskScheduler { get; set; }
        private TreeWalker TreeWalker { get; set; }
        private SCCMContentLibResolver SCCMResolver { get; set; }
        //private EffectivePermissions effectivePermissions { get; set; } = new EffectivePermissions(MyOptions.CurrentUser);

        public ShareFinder()
        {
            Mq = BlockingMq.GetMq();
            TreeTaskScheduler = SnaffCon.GetTreeTaskScheduler();
            TreeWalker = SnaffCon.GetTreeWalker();
            SCCMResolver = new SCCMContentLibResolver(MyOptions.LogLevelString == "debug" || MyOptions.LogLevelString == "trace");
        }

        internal void GetComputerShares(string computer)
        {
            // find the shares
            HostShareInfo[] hostShareInfos = GetHostShareInfo(computer);

            foreach (HostShareInfo hostShareInfo in hostShareInfos)
            {

                // skip IPC$ and PRINT$ shares for #OPSEC!!!
                List<string> neverScan = new List<string> { "ipc$", "print$" };
                if (neverScan.Contains(hostShareInfo.shi1_netname.ToLower()))
                {
                    continue;
                }

                string shareName = GetShareName(hostShareInfo, computer);
                if (!String.IsNullOrWhiteSpace(shareName))
                {
                    bool matched = false;

                    // SYSVOL and NETLOGON shares are replicated so they have special logic - do not use Classifiers for these
                    switch (hostShareInfo.shi1_netname.ToUpper())
                    {    
                        case "SYSVOL":
                            if (MyOptions.ScanSysvol == true)
                            {
                                //  Leave matched as false so that we don't suppress the TreeWalk for the first SYSVOL replica we see.
                                //  Toggle the flag so that any other shares replica will be skipped
                                MyOptions.ScanSysvol = false;
                                break;
                            }
                            matched = true;
                            break;
                        case "NETLOGON":
                            if (MyOptions.ScanNetlogon == true)
                            {                                
                                //  Same logic as SYSVOL above
                                MyOptions.ScanNetlogon = false;
                                break;
                            }
                            matched = true;
                            break;
                        default:
                            // classify them
                            foreach (ClassifierRule classifier in MyOptions.ShareClassifiers)
                            {
                                ShareClassifier shareClassifier = new ShareClassifier(classifier);
                                if (shareClassifier.ClassifyShare(shareName))
                                {
                                    // in this instance 'matched' means 'matched a discard rule, so don't send to treewalker'.
                                    matched = true;
                                    break;
                                }
                            }
                            break;
                    }

                    // by default all shares should go on to TreeWalker unless the classifier pulls them out.
                    // send them to TreeWalker
                    if (!matched)
                    {
                        // At least one classifier was matched so we will return this share to the results
                        ShareResult shareResult = new ShareResult()
                        {
                            Listable = true,
                            SharePath = shareName,
                            ShareComment = hostShareInfo.shi1_remark.ToString()
                        };

                        // Try to find this computer+share in the list of DFS targets


                        /*
                                                foreach (DFSShare dfsShare in MyOptions.DfsShares)
                                                {
                                                    ///TODO: Add some logic to match cases where short hostnames is used in DFS target list
                                                    if (dfsShare.RemoteServerName.Equals(computer, StringComparison.OrdinalIgnoreCase) &&
                                                        dfsShare.Name.Equals(hostShareInfo.shi1_netname, StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        // why the not operator?   if (!MyOptions.DfsNamespacePaths.Contains(dfsShare.DfsNamespacePath))
                                                        if (MyOptions.DfsNamespacePaths.Contains(dfsShare.DfsNamespacePath))
                                                        {
                                                            // remove the namespace path to make sure we don't kick it off again.
                                                            MyOptions.DfsNamespacePaths.Remove(dfsShare.DfsNamespacePath);
                                                            // sub out the \\computer\share path for the dfs namespace path. this makes sure we hit the most efficient endpoint. 
                                                            shareName = dfsShare.DfsNamespacePath;
                                                        }
                                                        else // if that dfs namespace has already been removed from our list, skip further scanning of that share.
                                                        {
                                                            skip = true;
                                                        }

                                                        // Found DFS target matching this computer+share - no further comparisons needed
                                                        break;
                                                    }
                                                }
                        */


                        // If this path can be accessed via DFS
                        if (MyOptions.DfsSharesDict.ContainsKey(shareName))
                        {                            
                            string dfsUncPath = MyOptions.DfsSharesDict[shareName];

                            Mq.Degub(String.Format("Matched host path {0} to DFS {1}",shareName, dfsUncPath));

                            // and if we haven't already scanned this share
                            if (MyOptions.DfsNamespacePaths.Contains(dfsUncPath))
                            {
                                Mq.Degub(String.Format("Will scan {0} using DFS referral instead of explicit host", dfsUncPath));

                                // sub out the \\computer\share path for the dfs namespace path. this makes sure we hit the most efficient endpoint. 
                                shareResult.SharePath = dfsUncPath;

                                // remove the namespace path to make sure we don't kick it off again.
                                MyOptions.DfsNamespacePaths.Remove(dfsUncPath);
                            }
                            else // if that dfs path has already been removed from our list, skip further scanning of that share.
                            {
                                // Do we want to report a gray share result for these?  I think not.
                                // Mq.ShareResult(shareResult);
                                break;
                            }
                        }

                        //  If the share is readable then dig deeper.
                        if (IsShareReadable(shareResult.SharePath))
                        {
                            // Share is readable, report as green  (the old default/min of the Triage enum )
                            shareResult.Triage = Triage.Green;

                            try
                            {
                                DirectoryInfo dirInfo = new DirectoryInfo(shareResult.SharePath);

                                //EffectivePermissions.RwStatus rwStatus = effectivePermissions.CanRw(dirInfo);

                                shareResult.RootModifyable = false;
                                shareResult.RootWritable = false;
                                shareResult.RootReadable = true;

                                /*
                                 if (rwStatus.CanWrite || rwStatus.CanModify)
                                {
                                    triage = Triage.Yellow;
                                }
                                */
                            }
                            catch (System.UnauthorizedAccessException e)
                            {
                                Mq.Error("Failed to get permissions on " + shareResult.SharePath);
                            }

                            if (MyOptions.ScanFoundShares)
                            {
                                if (SCCMResolver.IsSCCMShare(shareResult.SharePath))
                                {
                                    Mq.Trace("Creating a TreeWalker task for SCCM share " + shareResult.SharePath);

                                    TreeTaskScheduler.New(() =>
                                    {
                                        try
                                        {
                                            var resolvedPaths = SCCMResolver.ResolveSCCMContentPaths(shareResult.SharePath);

                                            if (resolvedPaths.Count > 0)
                                            {
                                                Mq.Degub($"Processing {resolvedPaths.Count} SCCM files with directory classifiers");

                                                // Group files by directory for directory-level classification
                                                var directoryMap = SCCMResolver.GroupFilesByDirectory(resolvedPaths);

                                                foreach (var directory in directoryMap.Keys)
                                                {
                                                    bool dirMatched = false;

                                                    // Apply directory classifiers
                                                    foreach (ClassifierRule dirRule in MyOptions.DirClassifiers)
                                                    {
                                                        DirClassifier dirClassifier = new DirClassifier(dirRule);
                                                        DirResult result = dirClassifier.ClassifyDir(directory);
                                                        if (result != null)  // Directory matched a rule
                                                        {
                                                            // Directory matched a discard rule, skip its files
                                                            dirMatched = true;
                                                            break;
                                                        }
                                                    }

                                                    // If directory wasn't filtered out, process its files
                                                    if (!dirMatched)
                                                    {
                                                        foreach (var filePath in directoryMap[directory])
                                                        {
                                                            TreeWalker.ProcessSCCMFile(filePath);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                Mq.Degub("No SCCM files resolved, using standard tree walk");
                                                TreeWalker.WalkTree(shareResult.SharePath);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Mq.Error("Exception in SCCM ContentLib processing for share " + shareResult.SharePath);
                                            Mq.Trace(e.ToString());
                                            Mq.Degub("SCCM resolution error, using standard tree walk");
                                            try
                                            {
                                                TreeWalker.WalkTree(shareResult.SharePath);
                                            }
                                            catch (Exception ex)
                                            {
                                                Mq.Error("Fallback tree walk also failed: " + ex.Message);
                                                Mq.Trace(ex.ToString());
                                            }
                                        }
                                    });
                                }
                                else
                                {
                                    Mq.Trace("Creating a TreeWalker task for " + shareResult.SharePath);
                                    TreeTaskScheduler.New(() =>
                                    {
                                        try
                                        {
                                            TreeWalker.WalkTree(shareResult.SharePath);
                                        }
                                        catch (Exception e)
                                        {
                                            Mq.Error("Exception in TreeWalker task for share " + shareResult.SharePath);
                                            Mq.Error(e.ToString());
                                        }
                                    });
                                }
                            }
                            Mq.ShareResult(shareResult);
                        }
                        else if (MyOptions.LogDeniedShares == true)
                        {
                            Mq.ShareResult(shareResult);
                        }
                    }
                }
            }
        }

        internal bool IsShareReadable(string share)
        {
            if (share.EndsWith("IPC$", StringComparison.OrdinalIgnoreCase) || share.EndsWith("print$", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            BlockingMq Mq = BlockingMq.GetMq();
            try
            {
                string[] files = Directory.GetFiles(share);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
            catch (Exception e)
            {
                Mq.Trace("Unhandled exception in IsShareReadable() for share path: " + share + " Full Exception:" + e.ToString());
            }
            return false;
        }

        private string GetShareName(HostShareInfo hostShareInfo, string computer)
        {
            // takes a HostShareInfo object and a computer name and turns it into a usable path.

            // first we want to throw away any errored out ones.
            string[] errors = { "ERROR=53", "ERROR=5" };
            if (errors.Contains(hostShareInfo.shi1_netname))
            {
                //Mq.Trace(hostShareInfo.shi1_netname + " on " + computer +
                //", but this is usually no cause for alarm.");
                return null;
            }

            Mq.Degub("Share discovered: " + $"\\\\{computer}\\{hostShareInfo.shi1_netname}");

            return $"\\\\{computer}\\{hostShareInfo.shi1_netname}";
        }

        private HostShareInfo[] GetHostShareInfo(string server)
        {
            // gets a share info object when given a host.
            List<HostShareInfo> shareInfos = new List<HostShareInfo>();
            int entriesread = 0;
            int totalentries = 0;
            int resumeHandle = 0;
            int nStructSize = Marshal.SizeOf(typeof(HostShareInfo));
            IntPtr bufPtr = IntPtr.Zero;
            int ret = NetShareEnum(new StringBuilder(server), 1, ref bufPtr, MaxPreferredLength, ref entriesread,
                ref totalentries,
                ref resumeHandle);
            if (ret == NerrSuccess)
            {
                IntPtr currentPtr = bufPtr;
                for (int i = 0; i < entriesread; i++)
                {
                    HostShareInfo shi1 = (HostShareInfo)Marshal.PtrToStructure(currentPtr, typeof(HostShareInfo));
                    shareInfos.Add(shi1);
                    currentPtr += nStructSize;
                }

                NetApiBufferFree(bufPtr);
                return shareInfos.ToArray();
            }

            shareInfos.Add(new HostShareInfo("ERROR=" + ret, 10, string.Empty));
            return shareInfos.ToArray();
        }

        // HERE BE WIN32 DRAGONS
        // ---------------------

        [DllImport("Netapi32.dll", SetLastError = true)]
        private static extern int NetWkstaGetInfo(string servername, int level, out IntPtr bufptr);

        [DllImport("Netapi32.dll", SetLastError = true)]
        private static extern int NetApiBufferFree(IntPtr buffer);

        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int NetShareEnum(
            StringBuilder serverName,
            int level,
            ref IntPtr bufPtr,
            uint prefmaxlen,
            ref int entriesread,
            ref int totalentries,
            ref int resumeHandle
        );

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WkstaInfo100
        {
            public int platform_id;
            public string computer_name;
            public string lan_group;
            public int ver_major;
            public int ver_minor;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct ShareInfo0
        {
            public string shi0_netname;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct HostShareInfo
        {
            public string shi1_netname;
            public uint shi1_type;
            public string shi1_remark;

            public HostShareInfo(string sharename, uint sharetype, string remark)
            {
                shi1_netname = sharename;
                shi1_type = sharetype;
                shi1_remark = remark;
            }

            public override string ToString()
            {
                return shi1_netname;
            }
        }

        private const uint MaxPreferredLength = 0xFFFFFFFF;
        private const int NerrSuccess = 0;

        private enum NetError : uint
        {
            NerrSuccess = 0,
            NerrBase = 2100,
            NerrUnknownDevDir = (NerrBase + 16),
            NerrDuplicateShare = (NerrBase + 18),
            NerrBufTooSmall = (NerrBase + 23)
        }

        private enum ShareType : uint
        {
            StypeDisktree = 0,
            StypePrintq = 1,
            StypeDevice = 2,
            StypeIpc = 3,
            StypeSpecial = 0x80000000
        }
    }
}
