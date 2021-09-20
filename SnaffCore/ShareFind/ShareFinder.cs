using Classifiers;
using SnaffCore.Concurrency;
using SnaffCore.TreeWalk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SnaffCore.Classifiers.EffectiveAccess;
using static SnaffCore.Config.Options;


namespace SnaffCore.ShareFind
{
    public class ShareFinder
    {
        private BlockingMq Mq { get; set; }
        private BlockingStaticTaskScheduler TreeTaskScheduler { get; set; }
        private TreeWalker TreeWalker { get; set; }
        private EffectivePermissions effectivePermissions { get; set; } = new EffectivePermissions();


        public ShareFinder()
        {
            Mq = BlockingMq.GetMq();
            TreeTaskScheduler = SnaffCon.GetTreeTaskScheduler();
            TreeWalker = SnaffCon.GetTreeWalker();
        }

        internal void GetComputerShares(string computer)
        {
            // find the shares
            HostShareInfo[] hostShareInfos = GetHostShareInfo(computer);

            foreach (HostShareInfo hostShareInfo in hostShareInfos)
            {
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
                                //  leave matched as false so that we don't suppress the TreeWalk for the first SYSVOL replica we see
                                //  toggle the flag so that any other shares replica will be skipped
                                MyOptions.ScanSysvol = false;
                                break;
                            }
                            matched = true;
                            break;
                        case "NETLOGON":
                            if (MyOptions.ScanNetlogon == true)
                            {                                
                                //  same as SYSVOL above
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
                        if (IsShareReadable(shareName))
                        {
                            DirectoryInfo dirInfo = new DirectoryInfo(shareName);
                            EffectivePermissions.RwStatus rwStatus = EffectivePermissions.CanRw(dirInfo);

                            Triage triage = Triage.Green;
                            if (rwStatus.CanWrite || rwStatus.CanModify)
                            {
                                triage = Triage.Yellow;
                            }

                            ShareResult shareResult = new ShareResult()
                            {
                                Listable = true,
                                Triage = triage,
                                RootModifyable = rwStatus.CanModify,
                                RootWritable = rwStatus.CanWrite,
                                RootReadable = rwStatus.CanRead,
                                SharePath = shareName,
                                ShareComment = hostShareInfo.shi1_remark.ToString()
                            };
                            Mq.ShareResult(shareResult);

                            if (MyOptions.ScanFoundShares)
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
                    }
                }
            }
        }

        internal bool IsShareReadable(string share)
        {
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
            catch (Exception e)
            {
                Mq.Trace(e.ToString());
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