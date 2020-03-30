﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SnaffCore.ShareFind
{
    public class ShareFinder
    {
        public class ShareResult
        {
            public string SharePath { get; set; }
            public bool Listable { get; set; }
            public bool IsAdminShare { get; set; }
        }

        internal List<string> GetReadableShares(string computer, Config.Config config)
        {
            var readableShares = new List<string>();
            var hostShareInfos = GetHostShareInfo(computer);
            foreach (var hostShareInfo in hostShareInfos)
            {
                var shareName = GetShareName(hostShareInfo, computer, config);
                if (!String.IsNullOrWhiteSpace(shareName))
                {
                    if (IsShareReadable(shareName, config))
                    {
                        readableShares.Add(shareName);
                    }
                }
            }

            return readableShares;
        }

        internal bool IsShareReadable(string share, Config.Config config)
        {
            try
            {
                var files = Directory.GetFiles(share);
                return true;
            }
            catch (Exception e)
            {
                config.Mq.Trace(e.ToString());
            }

            return false;
        }

        private string GetShareName(HostShareInfo hostShareInfo, string computer, Config.Config config)
        {
            // takes a HostShareInfo object and a computer name and turns it into a usable path.

            // first we want to throw away any errored out ones.
            string[] errors = {"ERROR=53", "ERROR=5"};
            if (errors.Contains(hostShareInfo.shi1_netname))
            {
                config.Mq.Trace(hostShareInfo.shi1_netname + " on " + computer +
                                ", but this is usually no cause for alarm.");
                return null;
            }

            return $"\\\\{computer}\\{hostShareInfo.shi1_netname}";
        }

        private HostShareInfo[] GetHostShareInfo(string server)
        {
            // gets a share info object when given a host.
            var shareInfos = new List<HostShareInfo>();
            var entriesread = 0;
            var totalentries = 0;
            var resumeHandle = 0;
            var nStructSize = Marshal.SizeOf(typeof(HostShareInfo));
            var bufPtr = IntPtr.Zero;
            var ret = NetShareEnum(new StringBuilder(server), 1, ref bufPtr, MaxPreferredLength, ref entriesread,
                ref totalentries,
                ref resumeHandle);
            if (ret == NerrSuccess)
            {
                var currentPtr = bufPtr;
                for (var i = 0; i < entriesread; i++)
                {
                    var shi1 = (HostShareInfo) Marshal.PtrToStructure(currentPtr, typeof(HostShareInfo));
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

        [DllImport("Netapi32.dll", SetLastError = true)]
        public static extern int NetWkstaGetInfo(string servername, int level, out IntPtr bufptr);

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