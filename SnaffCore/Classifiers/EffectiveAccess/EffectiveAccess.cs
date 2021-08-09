using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Collections;
using System.Security;
using System.Runtime.ConstrainedExecution;
using System.Runtime.CompilerServices;

// from Raimund Andree's NTFSSecurity PowerShell module https://www.powershellgallery.com/packages/NTFSSecurity/4.2.3

namespace SnaffCore.Classifiers.EffectiveAccess
{

    public class EffectivePermissions
    {
        public class RwStatus
        {
            public bool CanRead { get; set; }
            public bool CanWrite { get; set; }
            public bool CanModify { get; set; }
        }

        public static RwStatus CanRw(FileInfo fileInfo)
        {
            try
            {
                RwStatus rwStatus = new RwStatus { CanWrite = false, CanRead = false, CanModify = false };
                EffectivePermissions effPerms = new EffectivePermissions();
                string dir = fileInfo.DirectoryName;
                string hostname = "localhost";
                /*
                if (dir.StartsWith("\\\\"))
                {
                    hostname = dir.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                }
                */

                string whoami = WindowsIdentity.GetCurrent().Name;

                string[] accessStrings = effPerms.GetEffectivePermissions(whoami, fileInfo.FullName, false, hostname);


                string[] readRights = new string[] { "Read", "ReadAndExecute", "ReadData", "ListDirectory" };
                string[] writeRights = new string[] { "Write", "Modify", "FullControl", "TakeOwnership", "ChangePermissions", "AppendData", "WriteData", "CreateFiles", "CreateDirectories" };
                string[] modifyRights = new string[] { "Modify", "FullControl", "TakeOwnership", "ChangePermissions" };

                foreach (string access in accessStrings)
                {
                    if (access == "FullControl")
                    {
                        rwStatus.CanModify = true;
                        rwStatus.CanRead = true;
                        rwStatus.CanWrite = true;
                    }
                    if (readRights.Contains(access))
                    {
                        rwStatus.CanRead = true;
                    }
                    if (writeRights.Contains(access))
                    {
                        rwStatus.CanWrite = true;
                    }
                    if (modifyRights.Contains(access))
                    {
                        rwStatus.CanModify = true;
                    }
                }

                return rwStatus;
            }
            catch (Exception e)
            {
                return new RwStatus { CanWrite = false, CanRead = false, CanModify = false }; ;
            }
        }

        public string[] GetEffectivePermissions(string username, string path, bool isDirectory, string servername)
        {
            EffectiveAccessInfo effectiveAccessInfo;

            IdentityReference2 idRef2 = new IdentityReference2(username);

            if (isDirectory)
            {
                Alphaleonis.Win32.Filesystem.DirectoryInfo item = new Alphaleonis.Win32.Filesystem.DirectoryInfo(path); 
                effectiveAccessInfo = EffectiveAccess.GetEffectiveAccess(item, idRef2, servername);
            }
            else
            {
                Alphaleonis.Win32.Filesystem.FileInfo item = new Alphaleonis.Win32.Filesystem.FileInfo(path);
                effectiveAccessInfo = EffectiveAccess.GetEffectiveAccess(item, idRef2, servername);
            }

            string accesslist = effectiveAccessInfo.Ace.AccessRights.ToString();

            string[] accesslistarray = accesslist.Replace(" ", "").Split(',');

            return accesslistarray;
        }
    }
    public class EffectiveAccess
    {
        public static EffectiveAccessInfo GetEffectiveAccess(
          Alphaleonis.Win32.Filesystem.FileSystemInfo item,
          IdentityReference2 id,
          string serverName)
        {
            bool remoteServerAvailable = false;
            Exception authzException = (Exception)null;
            int effectiveAccess = new Win32().GetEffectiveAccess((ObjectSecurity)new FileSystemSecurity2(item).SecurityDescriptor, id, serverName, out remoteServerAvailable, out authzException);
            return new EffectiveAccessInfo(new FileSystemAccessRule2(new FileSystemAccessRule((IdentityReference)(SecurityIdentifier)id, (FileSystemRights)effectiveAccess, AccessControlType.Allow), item), remoteServerAvailable, authzException);
        }
    }

    public class EffectiveAccessInfo
    {
        private FileSystemAccessRule2 ace;
        private bool fromRemote;
        private Exception authzException;

        public FileSystemAccessRule2 Ace => this.ace;

        public bool FromRemote => this.fromRemote;

        public Exception AuthzException => this.authzException;

        public bool OperationFailed => this.authzException != null;

        public EffectiveAccessInfo(
          FileSystemAccessRule2 ace,
          bool fromRemote,
          Exception authzException = null)
        {
            this.ace = ace;
            this.fromRemote = fromRemote;
            this.authzException = authzException;
        }
    }

    public class IdentityReference2
    {
        protected static Regex sidValidation = new Regex("(S-1-)[0-9-]+", RegexOptions.IgnoreCase);
        protected SecurityIdentifier sid;
        protected NTAccount ntAccount;
        protected string lastError;

        public string Sid => this.sid.Value;

        public string AccountName => !(this.ntAccount != (NTAccount)null) ? string.Empty : this.ntAccount.Value;

        public string LastError => this.lastError;

        public IdentityReference2(IdentityReference ir)
        {
            this.ntAccount = ir as NTAccount;
            if (this.ntAccount != (NTAccount)null)
            {
                this.sid = (SecurityIdentifier)this.ntAccount.Translate(typeof(SecurityIdentifier));
            }
            else
            {
                this.sid = ir as SecurityIdentifier;
                if (!(this.sid != (SecurityIdentifier)null))
                    return;
                try
                {
                    this.ntAccount = (NTAccount)this.sid.Translate(typeof(NTAccount));
                }
                catch (Exception ex)
                {
                    this.lastError = ex.Message;
                }
            }
        }

        public IdentityReference2(string value)
        {
            Match match = !string.IsNullOrEmpty(value) ? IdentityReference2.sidValidation.Match(value) : throw new ArgumentException("The value cannot be empty");
            if (!string.IsNullOrEmpty(match.Value))
            {
                try
                {
                    this.sid = new SecurityIdentifier(match.Value);
                }
                catch (Exception ex)
                {
                    throw new InvalidCastException("Could not create an IdentityReference2 with the given SID", ex);
                }
                try
                {
                    this.ntAccount = (NTAccount)this.sid.Translate(typeof(NTAccount));
                }
                catch (Exception ex)
                {
                    this.lastError = ex.Message;
                }
            }
            else
            {
                try
                {
                    this.ntAccount = new NTAccount(value);
                    this.sid = (SecurityIdentifier)this.ntAccount.Translate(typeof(SecurityIdentifier));
                }
                catch (IdentityNotMappedException ex)
                {
                    throw ex;
                }
            }
        }

        public static explicit operator NTAccount(IdentityReference2 ir2) => ir2.ntAccount;

        public static explicit operator IdentityReference2(NTAccount ntAccount) => new IdentityReference2((IdentityReference)ntAccount);

        public static explicit operator SecurityIdentifier(IdentityReference2 ir2) => ir2.sid;

        public static explicit operator IdentityReference2(SecurityIdentifier sid) => new IdentityReference2((IdentityReference)sid);

        public static implicit operator IdentityReference(IdentityReference2 ir2) => (IdentityReference)ir2.sid;

        public static implicit operator IdentityReference2(IdentityReference ir) => new IdentityReference2(ir);

        public static implicit operator IdentityReference2(string value) => new IdentityReference2(value);

        public static implicit operator string(IdentityReference2 ir2) => ir2.ToString();

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if ((object)this == obj)
                return true;
            SecurityIdentifier securityIdentifier = obj as SecurityIdentifier;
            if (securityIdentifier != (SecurityIdentifier)null)
                return this.sid == securityIdentifier;
            NTAccount ntAccount = obj as NTAccount;
            if (ntAccount != (NTAccount)null)
                return this.ntAccount == ntAccount;
            IdentityReference2 identityReference2 = obj as IdentityReference2;
            if (identityReference2 != (IdentityReference2)null)
                return this.sid == identityReference2.sid;
            return obj is string str && (this.sid.Value == str || this.ntAccount != (NTAccount)null && this.ntAccount.Value.ToLower() == str.ToLower());
        }

        public override int GetHashCode() => this.sid.GetHashCode();

        public static bool operator ==(IdentityReference2 ir1, IdentityReference2 ir2)
        {
            if ((object)ir1 == (object)ir2)
                return true;
            return !((object)ir1 == null | (object)ir2 == null) && ir1.Equals((object)ir2);
        }

        public static bool operator !=(IdentityReference2 ir1, IdentityReference2 ir2)
        {
            if ((object)ir1 == (object)ir2)
                return false;
            return (object)ir1 == null | (object)ir2 == null || !ir1.Equals((object)ir2);
        }

        public byte[] GetBinaryForm()
        {
            byte[] binaryForm = new byte[this.sid.BinaryLength];
            this.sid.GetBinaryForm(binaryForm, 0);
            return binaryForm;
        }

        public override string ToString() => this.ntAccount == (NTAccount)null ? this.sid.ToString() : this.ntAccount.ToString();
    }

    internal class Win32
    {
        private const string ADVAPI32_DLL = "advapi32.dll";
        internal const string KERNEL32_DLL = "kernel32.dll";
        internal const string AUTHZ_DLL = "authz.dll";
        internal const string AUTHZ_OBJECTUUID_WITHCAP = "9a81c2bd-a525-471d-a4ed-49907c0b23da";
        internal const string RCP_OVER_TCP_PROTOCOL = "ncacn_ip_tcp";
        private IntPtr userClientCtxt = IntPtr.Zero;
        private SafeAuthzRMHandle authzRM;
        private IntPtr pGrantedAccess = IntPtr.Zero;
        private IntPtr pErrorSecObj = IntPtr.Zero;

        [DllImport("advapi32.dll", EntryPoint = "GetInheritanceSourceW", CharSet = CharSet.Unicode)]
        private static extern uint GetInheritanceSource(
          [MarshalAs(UnmanagedType.LPTStr)] string pObjectName,
          ResourceType ObjectType,
          SECURITY_INFORMATION SecurityInfo,
          [MarshalAs(UnmanagedType.Bool)] bool Container,
          IntPtr pObjectClassGuids,
          uint GuidCount,
          byte[] pAcl,
          IntPtr pfnArray,
          ref Win32.GENERIC_MAPPING pGenericMapping,
          IntPtr pInheritArray);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        private static extern uint FreeInheritedFromArray(
          IntPtr pInheritArray,
          ushort AceCnt,
          IntPtr pfnArray);

        [DllImport("authz.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AuthzInitializeRemoteResourceManager(
          IntPtr rpcInitInfo,
          out SafeAuthzRMHandle authRM);

        [DllImport("authz.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AuthzInitializeResourceManager(
          AuthzResourceManagerFlags flags,
          IntPtr pfnAccessCheck,
          IntPtr pfnComputeDynamicGroups,
          IntPtr pfnFreeDynamicGroups,
          string szResourceManagerName,
          out SafeAuthzRMHandle phAuthzResourceManager);

        [DllImport("authz.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AuthzInitializeContextFromSid(
          AuthzInitFlags flags,
          byte[] rawUserSid,
          SafeAuthzRMHandle authzRM,
          IntPtr expirationTime,
          Win32.LUID Identifier,
          IntPtr DynamicGroupArgs,
          out IntPtr authzClientContext);

        [DllImport("authz.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AuthzAccessCheck(
          Win32.AuthzACFlags flags,
          IntPtr hAuthzClientContext,
          ref Win32.AUTHZ_ACCESS_REQUEST pRequest,
          IntPtr AuditEvent,
          byte[] rawSecurityDescriptor,
          IntPtr[] OptionalSecurityDescriptorArray,
          uint OptionalSecurityDescriptorCount,
          ref Win32.AUTHZ_ACCESS_REPLY pReply,
          IntPtr cachedResults);

        [DllImport("authz.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AuthzFreeContext(IntPtr authzClientContext);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        public static extern uint GetSecurityDescriptorLength(IntPtr pSecurityDescriptor);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern uint GetSecurityInfo(
          SafeFileHandle handle,
          ObjectType objectType,
          SecurityInformationClass infoClass,
          IntPtr owner,
          IntPtr group,
          IntPtr dacl,
          IntPtr sacl,
          out IntPtr securityDescriptor);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern SafeFileHandle CreateFile(
          string lpFileName,
          FileAccess desiredAccess,
          FileShare shareMode,
          IntPtr lpSecurityAttributes,
          FileMode mode,
          FileFlagAttrib flagsAndAttributes,
          IntPtr hTemplateFile);

        public static List<string> GetInheritedFrom(Alphaleonis.Win32.Filesystem.FileSystemInfo item, ObjectSecurity sd)
        {
            List<string> stringList = new List<string>();
            RawSecurityDescriptor securityDescriptor = new RawSecurityDescriptor(sd.GetSecurityDescriptorBinaryForm(), 0);
            if (securityDescriptor.SystemAcl != null)
            {
                int count = securityDescriptor.SystemAcl.Count;
                byte[] numArray = new byte[securityDescriptor.SystemAcl.BinaryLength];
                securityDescriptor.SystemAcl.GetBinaryForm(numArray, 0);
                try
                {
                    stringList = Win32.GetInheritedFrom(item.FullName, numArray, count, item is Alphaleonis.Win32.Filesystem.DirectoryInfo, SECURITY_INFORMATION.SACL_SECURITY_INFORMATION);
                }
                catch
                {
                    stringList = new List<string>();
                    for (int index = 0; index < count; ++index)
                        stringList.Add("unknown parent");
                }
            }
            else if (securityDescriptor.DiscretionaryAcl != null)
            {
                int count = securityDescriptor.DiscretionaryAcl.Count;
                byte[] numArray = new byte[securityDescriptor.DiscretionaryAcl.BinaryLength];
                securityDescriptor.DiscretionaryAcl.GetBinaryForm(numArray, 0);
                try
                {
                    stringList = Win32.GetInheritedFrom(item.FullName, numArray, count, item is Alphaleonis.Win32.Filesystem.DirectoryInfo, SECURITY_INFORMATION.DACL_SECURITY_INFORMATION);
                }
                catch
                {
                    stringList = new List<string>();
                    for (int index = 0; index < count; ++index)
                        stringList.Add("unknown parent");
                }
            }
            return stringList;
        }

        public static List<string> GetInheritedFrom(
          string path,
          byte[] aclBytes,
          int aceCount,
          bool isContainer,
          SECURITY_INFORMATION aclType)
        {
            List<string> stringList = new List<string>();
            path = Alphaleonis.Win32.Filesystem.Path.GetLongPath(path);
            Win32.GENERIC_MAPPING pGenericMapping = new Win32.GENERIC_MAPPING();
            pGenericMapping.GenericRead = 1179785U;
            pGenericMapping.GenericWrite = 1179926U;
            pGenericMapping.GenericExecute = 1179808U;
            pGenericMapping.GenericAll = 2032127U;
            IntPtr num1 = Marshal.AllocHGlobal(aceCount * Marshal.SizeOf(typeof(Win32.PINHERITED_FROM)));
            uint inheritanceSource = Win32.GetInheritanceSource(path, ResourceType.FileObject, aclType, isContainer, IntPtr.Zero, 0U, aclBytes, IntPtr.Zero, ref pGenericMapping, num1);
            if (inheritanceSource != 0U)
                throw new Win32Exception((int)inheritanceSource);
            for (int index = 0; index < aceCount; ++index)
            {
                Win32.PINHERITED_FROM pinheritedFrom = num1.ElementAt<Win32.PINHERITED_FROM>(index);
                stringList.Add(string.IsNullOrEmpty(pinheritedFrom.AncestorName) || !pinheritedFrom.AncestorName.StartsWith("\\\\?\\") ? pinheritedFrom.AncestorName : pinheritedFrom.AncestorName.Substring(4));
            }
            int num2 = (int)Win32.FreeInheritedFromArray(num1, (ushort)aceCount, IntPtr.Zero);
            Marshal.FreeHGlobal(num1);
            return stringList;
        }

        public int GetEffectiveAccess(
          ObjectSecurity sd,
          IdentityReference2 identity,
          string serverName,
          out bool remoteServerAvailable,
          out Exception authzException)
        {
            int num = 0;
            remoteServerAvailable = false;
            authzException = (Exception)null;
            try
            {
                this.GetEffectivePermissions_AuthzInitializeResourceManager(serverName, out remoteServerAvailable);
                try
                {
                    this.GetEffectivePermissions_AuthzInitializeContextFromSid(identity);
                    num = this.GetEffectivePermissions_AuthzAccessCheck(sd);
                }
                catch (Exception ex)
                {
                    authzException = ex;
                }
            }
            catch
            {
            }
            finally
            {
                this.GetEffectivePermissions_FreeResouces();
            }
            return num;
        }

        private void GetEffectivePermissions_AuthzInitializeResourceManager(
          string serverName,
          out bool remoteServerAvailable)
        {
            remoteServerAvailable = false;
            if (!Win32.AuthzInitializeRemoteResourceManager(SafeHGlobalHandle.AllocHGlobalStruct<Win32.AUTHZ_RPC_INIT_INFO_CLIENT>(new Win32.AUTHZ_RPC_INIT_INFO_CLIENT()
            {
                version = AuthzRpcClientVersion.V1,
                objectUuid = "9a81c2bd-a525-471d-a4ed-49907c0b23da",
                protocol = "ncacn_ip_tcp",
                server = serverName
            }).ToIntPtr(), out this.authzRM))
            {
                int lastWin32Error = Marshal.GetLastWin32Error();
                if (lastWin32Error != 1753)
                    throw new Win32Exception(lastWin32Error);
                if (serverName == "localhost")
                    remoteServerAvailable = true;
                if (!Win32.AuthzInitializeResourceManager(AuthzResourceManagerFlags.NO_AUDIT, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, "EffectiveAccessCheck", out this.authzRM))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            else
                remoteServerAvailable = true;
        }

        private void GetEffectivePermissions_AuthzInitializeContextFromSid(IdentityReference2 id)
        {
            if (Win32.AuthzInitializeContextFromSid(AuthzInitFlags.Default, id.GetBinaryForm(), this.authzRM, IntPtr.Zero, Win32.LUID.NullLuid, IntPtr.Zero, out this.userClientCtxt))
                return;
            Win32Exception win32Exception = new Win32Exception(Marshal.GetLastWin32Error());
            if (win32Exception.NativeErrorCode != 1722)
                throw win32Exception;
        }

        private int GetEffectivePermissions_AuthzAccessCheck(ObjectSecurity sd)
        {
            Win32.AUTHZ_ACCESS_REQUEST pRequest = new Win32.AUTHZ_ACCESS_REQUEST();
            pRequest.DesiredAccess = StdAccess.MAXIMUM_ALLOWED;
            pRequest.PrincipalSelfSid = (byte[])null;
            pRequest.ObjectTypeList = IntPtr.Zero;
            pRequest.ObjectTypeListLength = 0;
            pRequest.OptionalArguments = IntPtr.Zero;
            Win32.AUTHZ_ACCESS_REPLY pReply = new Win32.AUTHZ_ACCESS_REPLY();
            pReply.ResultListLength = 1;
            pReply.SaclEvaluationResults = IntPtr.Zero;
            pReply.GrantedAccessMask = this.pGrantedAccess = Marshal.AllocHGlobal(4);
            pReply.Error = this.pErrorSecObj = Marshal.AllocHGlobal(4);
            byte[] descriptorBinaryForm = sd.GetSecurityDescriptorBinaryForm();
            if (!Win32.AuthzAccessCheck(Win32.AuthzACFlags.None, this.userClientCtxt, ref pRequest, IntPtr.Zero, descriptorBinaryForm, (IntPtr[])null, 0U, ref pReply, IntPtr.Zero) && Marshal.GetLastWin32Error() != 0)
                throw new Win32Exception();
            return Marshal.ReadInt32(this.pGrantedAccess);
        }

        private void GetEffectivePermissions_FreeResouces()
        {
            Marshal.FreeHGlobal(this.pGrantedAccess);
            Marshal.FreeHGlobal(this.pErrorSecObj);
            if (!(this.userClientCtxt != IntPtr.Zero))
                return;
            Win32.AuthzFreeContext(this.userClientCtxt);
            this.userClientCtxt = IntPtr.Zero;
        }

        private static RawSecurityDescriptor GetRawSecurityDescriptor(
          SafeFileHandle handle,
          SecurityInformationClass infoClass)
        {
            return new RawSecurityDescriptor(Win32.GetByteSecurityDescriptor(handle, infoClass), 0);
        }

        public static byte[] GetByteSecurityDescriptor(
          SafeFileHandle handle,
          SecurityInformationClass infoClass)
        {
            IntPtr securityDescriptor = IntPtr.Zero;
            byte[] destination = new byte[0];
            try
            {
                if (Win32.GetSecurityInfo(handle, ObjectType.File, infoClass, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, out securityDescriptor) != 0U)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                uint descriptorLength = Win32.GetSecurityDescriptorLength(securityDescriptor);
                destination = new byte[(int)descriptorLength];
                Marshal.Copy(securityDescriptor, destination, 0, (int)descriptorLength);
            }
            finally
            {
                Marshal.FreeHGlobal(securityDescriptor);
                IntPtr zero = IntPtr.Zero;
            }
            return destination;
        }

        private struct PINHERITED_FROM
        {
            public int GenerationGap;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string AncestorName;
        }

        private struct GENERIC_MAPPING
        {
            public uint GenericRead;
            public uint GenericWrite;
            public uint GenericExecute;
            public uint GenericAll;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct AUTHZ_RPC_INIT_INFO_CLIENT
        {
            public AuthzRpcClientVersion version;
            public string objectUuid;
            public string protocol;
            public string server;
            public string endPoint;
            public string options;
            public string serverSpn;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct LUID
        {
            public uint LowPart;
            public uint HighPart;

            public static Win32.LUID NullLuid
            {
                get
                {
                    Win32.LUID luid;
                    luid.LowPart = 0U;
                    luid.HighPart = 0U;
                    return luid;
                }
            }
        }

        internal struct AUTHZ_ACCESS_REQUEST
        {
            public StdAccess DesiredAccess;
            public byte[] PrincipalSelfSid;
            public IntPtr ObjectTypeList;
            public int ObjectTypeListLength;
            public IntPtr OptionalArguments;
        }

        internal struct AUTHZ_ACCESS_REPLY
        {
            public int ResultListLength;
            public IntPtr GrantedAccessMask;
            public IntPtr SaclEvaluationResults;
            public IntPtr Error;
        }

        internal enum AuthzACFlags : uint
        {
            None,
            NoDeepCopySD,
        }
    }
    public class FileSystemSecurity2
    {
        protected FileSecurity fileSecurityDescriptor;
        protected DirectorySecurity directorySecurityDescriptor;
        protected Alphaleonis.Win32.Filesystem.FileSystemInfo item;
        protected FileSystemSecurity sd;
        protected AccessControlSections sections;
        protected bool isFile;

        public Alphaleonis.Win32.Filesystem.FileSystemInfo Item
        {
            get => this.item;
            set => this.item = value;
        }

        public string FullName => this.item.FullName;

        public string Name => this.item.Name;

        public bool IsFile => this.isFile;

        public FileSystemSecurity2(Alphaleonis.Win32.Filesystem.FileSystemInfo item, AccessControlSections sections)
        {
            this.sections = sections;
            if (item is Alphaleonis.Win32.Filesystem.FileInfo)
            {
                this.item = item;
                this.sd = (FileSystemSecurity)((Alphaleonis.Win32.Filesystem.FileInfo)this.item).GetAccessControl(sections);
                this.isFile = true;
            }
            else
            {
                this.item = item;
                this.sd = (FileSystemSecurity)((Alphaleonis.Win32.Filesystem.DirectoryInfo)this.item).GetAccessControl(sections);
            }
        }

        public FileSystemSecurity2(Alphaleonis.Win32.Filesystem.FileSystemInfo item)
        {
            if (item is Alphaleonis.Win32.Filesystem.FileInfo)
            {
                this.item = item;
                try
                {
                    this.sd = (FileSystemSecurity)((Alphaleonis.Win32.Filesystem.FileInfo)this.item).GetAccessControl(AccessControlSections.All);
                }
                catch
                {
                    try
                    {
                        this.sd = (FileSystemSecurity)((Alphaleonis.Win32.Filesystem.FileInfo)this.item).GetAccessControl(AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
                    }
                    catch
                    {
                        this.sd = (FileSystemSecurity)((Alphaleonis.Win32.Filesystem.FileInfo)this.item).GetAccessControl(AccessControlSections.Access);
                    }
                }
                this.isFile = true;
            }
            else
            {
                this.item = item;
                try
                {
                    this.sd = (FileSystemSecurity)((Alphaleonis.Win32.Filesystem.DirectoryInfo)this.item).GetAccessControl(AccessControlSections.All);
                }
                catch
                {
                    try
                    {
                        this.sd = (FileSystemSecurity)((Alphaleonis.Win32.Filesystem.DirectoryInfo)this.item).GetAccessControl(AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
                    }
                    catch
                    {
                        this.sd = (FileSystemSecurity)((Alphaleonis.Win32.Filesystem.DirectoryInfo)this.item).GetAccessControl(AccessControlSections.Access);
                    }
                }
            }
        }

        public FileSystemSecurity SecurityDescriptor => this.sd;

        public void Write()
        {
            if (this.isFile)
                ((Alphaleonis.Win32.Filesystem.FileInfo)this.item).SetAccessControl((FileSecurity)this.sd);
            else
                ((Alphaleonis.Win32.Filesystem.DirectoryInfo)this.item).SetAccessControl((DirectorySecurity)this.sd);
        }

        public void Write(Alphaleonis.Win32.Filesystem.FileSystemInfo item)
        {
            if (item is Alphaleonis.Win32.Filesystem.FileInfo)
                ((Alphaleonis.Win32.Filesystem.FileInfo)item).SetAccessControl((FileSecurity)this.sd);
            else
                ((Alphaleonis.Win32.Filesystem.DirectoryInfo)item).SetAccessControl((DirectorySecurity)this.sd);
        }

        public void Write(string path)
        {
            Alphaleonis.Win32.Filesystem.FileSystemInfo fileSystemInfo;
            if (Alphaleonis.Win32.Filesystem.File.Exists(path))
                fileSystemInfo = (Alphaleonis.Win32.Filesystem.FileSystemInfo)new Alphaleonis.Win32.Filesystem.FileInfo(path);
            else
                fileSystemInfo = Alphaleonis.Win32.Filesystem.Directory.Exists(path) ? (Alphaleonis.Win32.Filesystem.FileSystemInfo)new Alphaleonis.Win32.Filesystem.DirectoryInfo(path) : throw new FileNotFoundException("File not found", path);
            this.Write(fileSystemInfo);
        }

        public static implicit operator FileSecurity(FileSystemSecurity2 fs2) => fs2.fileSecurityDescriptor;

        public static implicit operator FileSystemSecurity2(FileSecurity fs) => new FileSystemSecurity2((Alphaleonis.Win32.Filesystem.FileSystemInfo)new Alphaleonis.Win32.Filesystem.FileInfo(""));

        public static implicit operator DirectorySecurity(FileSystemSecurity2 fs2) => fs2.directorySecurityDescriptor;

        public static implicit operator FileSystemSecurity2(DirectorySecurity fs) => new FileSystemSecurity2((Alphaleonis.Win32.Filesystem.FileSystemInfo)new Alphaleonis.Win32.Filesystem.DirectoryInfo(""));

        public override bool Equals(object obj) => this.fileSecurityDescriptor == (FileSecurity)obj;

        public override int GetHashCode() => this.fileSecurityDescriptor.GetHashCode();

        public static void ConvertToFileSystemFlags(
          ApplyTo ApplyTo,
          out InheritanceFlags inheritanceFlags,
          out PropagationFlags propagationFlags)
        {
            inheritanceFlags = InheritanceFlags.None;
            propagationFlags = PropagationFlags.None;
            switch (ApplyTo)
            {
                case ApplyTo.ThisFolderOnly:
                    inheritanceFlags = InheritanceFlags.None;
                    propagationFlags = PropagationFlags.None;
                    break;
                case ApplyTo.ThisFolderSubfoldersAndFiles:
                    inheritanceFlags = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;
                    propagationFlags = PropagationFlags.None;
                    break;
                case ApplyTo.ThisFolderAndSubfolders:
                    inheritanceFlags = InheritanceFlags.ContainerInherit;
                    propagationFlags = PropagationFlags.None;
                    break;
                case ApplyTo.ThisFolderAndFiles:
                    inheritanceFlags = InheritanceFlags.ObjectInherit;
                    propagationFlags = PropagationFlags.None;
                    break;
                case ApplyTo.SubfoldersAndFilesOnly:
                    inheritanceFlags = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;
                    propagationFlags = PropagationFlags.InheritOnly;
                    break;
                case ApplyTo.SubfoldersOnly:
                    inheritanceFlags = InheritanceFlags.ContainerInherit;
                    propagationFlags = PropagationFlags.InheritOnly;
                    break;
                case ApplyTo.FilesOnly:
                    inheritanceFlags = InheritanceFlags.ObjectInherit;
                    propagationFlags = PropagationFlags.InheritOnly;
                    break;
                case ApplyTo.ThisFolderSubfoldersAndFilesOneLevel:
                    inheritanceFlags = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;
                    propagationFlags = PropagationFlags.NoPropagateInherit;
                    break;
                case ApplyTo.ThisFolderAndSubfoldersOneLevel:
                    inheritanceFlags = InheritanceFlags.ContainerInherit;
                    propagationFlags = PropagationFlags.NoPropagateInherit;
                    break;
                case ApplyTo.ThisFolderAndFilesOneLevel:
                    inheritanceFlags = InheritanceFlags.ObjectInherit;
                    propagationFlags = PropagationFlags.NoPropagateInherit;
                    break;
                case ApplyTo.SubfoldersAndFilesOnlyOneLevel:
                    inheritanceFlags = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;
                    propagationFlags = PropagationFlags.NoPropagateInherit | PropagationFlags.InheritOnly;
                    break;
                case ApplyTo.SubfoldersOnlyOneLevel:
                    inheritanceFlags = InheritanceFlags.ContainerInherit;
                    propagationFlags = PropagationFlags.NoPropagateInherit | PropagationFlags.InheritOnly;
                    break;
                case ApplyTo.FilesOnlyOneLevel:
                    inheritanceFlags = InheritanceFlags.ObjectInherit;
                    propagationFlags = PropagationFlags.NoPropagateInherit | PropagationFlags.InheritOnly;
                    break;
            }
        }

        public static ApplyTo ConvertToApplyTo(
          InheritanceFlags InheritanceFlags,
          PropagationFlags PropagationFlags)
        {
            if (InheritanceFlags == InheritanceFlags.ObjectInherit & PropagationFlags == PropagationFlags.InheritOnly)
                return ApplyTo.FilesOnly;
            if (InheritanceFlags == (InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit) & PropagationFlags == PropagationFlags.InheritOnly)
                return ApplyTo.SubfoldersAndFilesOnly;
            if (InheritanceFlags == InheritanceFlags.ContainerInherit & PropagationFlags == PropagationFlags.InheritOnly)
                return ApplyTo.SubfoldersOnly;
            if (InheritanceFlags == InheritanceFlags.ObjectInherit & PropagationFlags == PropagationFlags.None)
                return ApplyTo.ThisFolderAndFiles;
            if (InheritanceFlags == InheritanceFlags.ContainerInherit & PropagationFlags == PropagationFlags.None)
                return ApplyTo.ThisFolderAndSubfolders;
            if (InheritanceFlags == InheritanceFlags.None & PropagationFlags == PropagationFlags.None)
                return ApplyTo.ThisFolderOnly;
            if (InheritanceFlags == (InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit) & PropagationFlags == PropagationFlags.None)
                return ApplyTo.ThisFolderSubfoldersAndFiles;
            if (InheritanceFlags == (InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit) & PropagationFlags == PropagationFlags.NoPropagateInherit)
                return ApplyTo.ThisFolderSubfoldersAndFilesOneLevel;
            if (InheritanceFlags == InheritanceFlags.ContainerInherit & PropagationFlags == PropagationFlags.NoPropagateInherit)
                return ApplyTo.ThisFolderAndSubfoldersOneLevel;
            if (InheritanceFlags == InheritanceFlags.ObjectInherit & PropagationFlags == PropagationFlags.NoPropagateInherit)
                return ApplyTo.ThisFolderAndFilesOneLevel;
            if (InheritanceFlags == (InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit) & PropagationFlags == (PropagationFlags.NoPropagateInherit | PropagationFlags.InheritOnly))
                return ApplyTo.SubfoldersAndFilesOnlyOneLevel;
            if (InheritanceFlags == InheritanceFlags.ContainerInherit & PropagationFlags == (PropagationFlags.NoPropagateInherit | PropagationFlags.InheritOnly))
                return ApplyTo.SubfoldersOnlyOneLevel;
            if (InheritanceFlags == InheritanceFlags.ObjectInherit & PropagationFlags == (PropagationFlags.NoPropagateInherit | PropagationFlags.InheritOnly))
                return ApplyTo.FilesOnlyOneLevel;
            throw new RightsConverionException("The combination of InheritanceFlags and PropagationFlags could not be translated");
        }

        public static FileSystemRights MapGenericRightsToFileSystemRights(
          uint originalRights)
        {
            try
            {
                return !(Enum.Parse(typeof(FileSystemRights), originalRights.ToString()).ToString() == originalRights.ToString()) ? (FileSystemRights)originalRights : throw new ArgumentOutOfRangeException();
            }
            catch (Exception ex)
            {
                FileSystemRights fileSystemRights1 = (FileSystemRights)0;
                if (Convert.ToBoolean(originalRights & 536870912U))
                {
                    fileSystemRights1 |= FileSystemRights.ExecuteFile | FileSystemRights.ReadAttributes | FileSystemRights.ReadPermissions | FileSystemRights.Synchronize;
                    originalRights ^= 536870912U;
                }
                if (Convert.ToBoolean(originalRights & 2147483648U))
                {
                    fileSystemRights1 |= FileSystemRights.Read | FileSystemRights.Synchronize;
                    originalRights ^= 2147483648U;
                }
                if (Convert.ToBoolean(originalRights & 1073741824U))
                {
                    fileSystemRights1 |= FileSystemRights.Write | FileSystemRights.ReadPermissions | FileSystemRights.Synchronize;
                    originalRights ^= 1073741824U;
                }
                if (Convert.ToBoolean(originalRights & 268435456U))
                {
                    fileSystemRights1 |= FileSystemRights.FullControl;
                    originalRights ^= 268435456U;
                }
                FileSystemRights fileSystemRights2 = (FileSystemRights)Enum.Parse(typeof(FileSystemRights), originalRights.ToString());
                return fileSystemRights1 | fileSystemRights2;
            }
        }
    }

    public class FileSystemAccessRule2
    {
        private FileSystemAccessRule fileSystemAccessRule;
        private string fullName;
        private bool inheritanceEnabled;
        private string inheritedFrom;

        public string Name => System.IO.Path.GetFileName(this.fullName);

        public string FullName
        {
            get => this.fullName;
            set => this.fullName = value;
        }

        public bool InheritanceEnabled
        {
            get => this.inheritanceEnabled;
            set => this.inheritanceEnabled = value;
        }

        public string InheritedFrom
        {
            get => this.inheritedFrom;
            set => this.inheritedFrom = value;
        }

        public AccessControlType AccessControlType => this.fileSystemAccessRule.AccessControlType;

        public FileSystemRights2 AccessRights => (FileSystemRights2)this.fileSystemAccessRule.FileSystemRights;

        public IdentityReference2 Account => (IdentityReference2)this.fileSystemAccessRule.IdentityReference;

        public InheritanceFlags InheritanceFlags => this.fileSystemAccessRule.InheritanceFlags;

        public bool IsInherited => this.fileSystemAccessRule.IsInherited;

        public PropagationFlags PropagationFlags => this.fileSystemAccessRule.PropagationFlags;

        public FileSystemAccessRule2(FileSystemAccessRule fileSystemAccessRule) => this.fileSystemAccessRule = fileSystemAccessRule;

        public FileSystemAccessRule2(FileSystemAccessRule fileSystemAccessRule, Alphaleonis.Win32.Filesystem.FileSystemInfo item)
        {
            this.fileSystemAccessRule = fileSystemAccessRule;
            this.fullName = item.FullName;
        }

        public FileSystemAccessRule2(FileSystemAccessRule fileSystemAccessRule, string path) => this.fileSystemAccessRule = fileSystemAccessRule;

        public static implicit operator FileSystemAccessRule(
          FileSystemAccessRule2 ace2)
        {
            return ace2.fileSystemAccessRule;
        }

        public static implicit operator FileSystemAccessRule2(
          FileSystemAccessRule ace)
        {
            return new FileSystemAccessRule2(ace);
        }

        public override bool Equals(object obj) => this.fileSystemAccessRule == (FileSystemAccessRule)obj;

        public override int GetHashCode() => this.fileSystemAccessRule.GetHashCode();

        public override string ToString() => string.Format("{0} '{1}' ({2})", (object)this.AccessControlType.ToString()[0], (object)this.Account.AccountName, (object)this.AccessRights.ToString());

        public SimpleFileSystemAccessRule ToSimpleFileSystemAccessRule2() => new SimpleFileSystemAccessRule(this.fullName, this.Account, this.AccessRights);

        public static void RemoveFileSystemAccessRuleAll(
          FileSystemSecurity2 sd,
          List<IdentityReference2> accounts = null)
        {
            AuthorizationRuleCollection accessRules = sd.SecurityDescriptor.GetAccessRules(true, false, typeof(SecurityIdentifier));
            if (accounts != null)
                accessRules.OfType<FileSystemAccessRule>().Where<FileSystemAccessRule>((Func<FileSystemAccessRule, bool>)(ace => accounts.Where<IdentityReference2>((Func<IdentityReference2, bool>)(account => account == (IdentityReference2)ace.IdentityReference)).Count<IdentityReference2>() > 1));
            foreach (FileSystemAccessRule rule in (ReadOnlyCollectionBase)accessRules)
                sd.SecurityDescriptor.RemoveAccessRuleSpecific(rule);
        }

        public static void RemoveFileSystemAccessRuleAll(
          Alphaleonis.Win32.Filesystem.FileSystemInfo item,
          List<IdentityReference2> accounts = null)
        {
            FileSystemSecurity2 sd = new FileSystemSecurity2(item);
            FileSystemAccessRule2.RemoveFileSystemAccessRuleAll(sd, accounts);
            sd.Write();
        }

        public static void RemoveFileSystemAccessRule(
          Alphaleonis.Win32.Filesystem.FileSystemInfo item,
          IdentityReference2 account,
          FileSystemRights2 rights,
          AccessControlType type,
          InheritanceFlags inheritanceFlags,
          PropagationFlags propagationFlags,
          bool removeSpecific = false)
        {
            if (type == AccessControlType.Allow)
                rights |= FileSystemRights2.Synchronize;
            if ((Alphaleonis.Win32.Filesystem.FileSystemInfo)(item as Alphaleonis.Win32.Filesystem.FileInfo) != (Alphaleonis.Win32.Filesystem.FileSystemInfo)null)
            {
                Alphaleonis.Win32.Filesystem.FileInfo fileInfo = (Alphaleonis.Win32.Filesystem.FileInfo)item;
                FileSecurity accessControl = fileInfo.GetAccessControl(AccessControlSections.Access);
                FileSystemAccessRule rule = (FileSystemAccessRule)accessControl.AccessRuleFactory((IdentityReference)account, (int)rights, false, inheritanceFlags, propagationFlags, type);
                if (removeSpecific)
                    accessControl.RemoveAccessRuleSpecific(rule);
                else
                    accessControl.RemoveAccessRule(rule);
                fileInfo.SetAccessControl(accessControl);
            }
            else
            {
                Alphaleonis.Win32.Filesystem.DirectoryInfo directoryInfo = (Alphaleonis.Win32.Filesystem.DirectoryInfo)item;
                DirectorySecurity accessControl = directoryInfo.GetAccessControl(AccessControlSections.Access);
                FileSystemAccessRule rule = (FileSystemAccessRule)accessControl.AccessRuleFactory((IdentityReference)account, (int)rights, false, inheritanceFlags, propagationFlags, type);
                if (removeSpecific)
                    accessControl.RemoveAccessRuleSpecific(rule);
                else
                    accessControl.RemoveAccessRule(rule);
                directoryInfo.SetAccessControl(accessControl);
            }
        }

        public static void RemoveFileSystemAccessRule(
          Alphaleonis.Win32.Filesystem.FileSystemInfo item,
          List<IdentityReference2> accounts,
          FileSystemRights2 rights,
          AccessControlType type,
          InheritanceFlags inheritanceFlags,
          PropagationFlags propagationFlags,
          bool removeSpecific = false)
        {
            foreach (IdentityReference2 account in accounts)
                FileSystemAccessRule2.RemoveFileSystemAccessRule(item, account, rights, type, inheritanceFlags, propagationFlags, removeSpecific);
        }

        public static void RemoveFileSystemAccessRule(
          string path,
          IdentityReference2 account,
          FileSystemRights2 rights,
          AccessControlType type,
          InheritanceFlags inheritanceFlags,
          PropagationFlags propagationFlags,
          bool removeSpecific = false)
        {
            if (Alphaleonis.Win32.Filesystem.File.Exists(path))
                FileSystemAccessRule2.RemoveFileSystemAccessRule((Alphaleonis.Win32.Filesystem.FileSystemInfo)new Alphaleonis.Win32.Filesystem.FileInfo(path), account, rights, type, inheritanceFlags, propagationFlags, removeSpecific);
            else
                FileSystemAccessRule2.RemoveFileSystemAccessRule((Alphaleonis.Win32.Filesystem.FileSystemInfo)new Alphaleonis.Win32.Filesystem.DirectoryInfo(path), account, rights, type, inheritanceFlags, propagationFlags, removeSpecific);
        }

        public static void RemoveFileSystemAccessRule(
          string path,
          List<IdentityReference2> account,
          FileSystemRights2 rights,
          AccessControlType type,
          InheritanceFlags inheritanceFlags,
          PropagationFlags propagationFlags,
          bool removeSpecific = false)
        {
            if (Alphaleonis.Win32.Filesystem.File.Exists(path))
                FileSystemAccessRule2.RemoveFileSystemAccessRule((Alphaleonis.Win32.Filesystem.FileSystemInfo)new Alphaleonis.Win32.Filesystem.FileInfo(path), account, rights, type, inheritanceFlags, propagationFlags, removeSpecific);
            else
                FileSystemAccessRule2.RemoveFileSystemAccessRule((Alphaleonis.Win32.Filesystem.FileSystemInfo)new Alphaleonis.Win32.Filesystem.DirectoryInfo(path), account, rights, type, inheritanceFlags, propagationFlags, removeSpecific);
        }

        public static void RemoveFileSystemAccessRule(
          Alphaleonis.Win32.Filesystem.FileSystemInfo item,
          FileSystemAccessRule ace,
          bool removeSpecific = false)
        {
            if ((Alphaleonis.Win32.Filesystem.FileSystemInfo)(item as Alphaleonis.Win32.Filesystem.FileInfo) != (Alphaleonis.Win32.Filesystem.FileSystemInfo)null)
            {
                Alphaleonis.Win32.Filesystem.FileInfo fileInfo = (Alphaleonis.Win32.Filesystem.FileInfo)item;
                FileSecurity accessControl = fileInfo.GetAccessControl(AccessControlSections.Access);
                if (removeSpecific)
                    accessControl.RemoveAccessRuleSpecific(ace);
                else
                    accessControl.RemoveAccessRule(ace);
                fileInfo.SetAccessControl(accessControl);
            }
            else
            {
                Alphaleonis.Win32.Filesystem.DirectoryInfo directoryInfo = (Alphaleonis.Win32.Filesystem.DirectoryInfo)item;
                DirectorySecurity accessControl = directoryInfo.GetAccessControl(AccessControlSections.Access);
                if (removeSpecific)
                    accessControl.RemoveAccessRuleSpecific(ace);
                else
                    accessControl.RemoveAccessRule(ace);
                directoryInfo.SetAccessControl(accessControl);
            }
        }

        public static FileSystemAccessRule2 RemoveFileSystemAccessRule(
          FileSystemSecurity2 sd,
          IdentityReference2 account,
          FileSystemRights2 rights,
          AccessControlType type,
          InheritanceFlags inheritanceFlags,
          PropagationFlags propagationFlags,
          bool removeSpecific = false)
        {
            if (type == AccessControlType.Allow)
                rights |= FileSystemRights2.Synchronize;
            FileSystemAccessRule rule = (FileSystemAccessRule)sd.SecurityDescriptor.AccessRuleFactory((IdentityReference)account, (int)rights, false, inheritanceFlags, propagationFlags, type);
            if (sd.IsFile)
            {
                if (removeSpecific)
                    sd.SecurityDescriptor.RemoveAccessRuleSpecific(rule);
                else
                    sd.SecurityDescriptor.RemoveAccessRule(rule);
            }
            else if (removeSpecific)
                sd.SecurityDescriptor.RemoveAccessRuleSpecific(rule);
            else
                sd.SecurityDescriptor.RemoveAccessRule(rule);
            return (FileSystemAccessRule2)rule;
        }

        public static IEnumerable<FileSystemAccessRule2> RemoveFileSystemAccessRule(
          FileSystemSecurity2 sd,
          List<IdentityReference2> accounts,
          FileSystemRights2 rights,
          AccessControlType type,
          InheritanceFlags inheritanceFlags,
          PropagationFlags propagationFlags,
          bool removeSpecific = false)
        {
            List<FileSystemAccessRule2> systemAccessRule2List = new List<FileSystemAccessRule2>();
            foreach (IdentityReference2 account in accounts)
                systemAccessRule2List.Add(FileSystemAccessRule2.RemoveFileSystemAccessRule(sd, account, rights, type, inheritanceFlags, propagationFlags));
            return (IEnumerable<FileSystemAccessRule2>)systemAccessRule2List;
        }

        public static IEnumerable<FileSystemAccessRule2> GetFileSystemAccessRules(
          Alphaleonis.Win32.Filesystem.FileSystemInfo item,
          bool includeExplicit,
          bool includeInherited,
          bool getInheritedFrom = false)
        {
            return FileSystemAccessRule2.GetFileSystemAccessRules(new FileSystemSecurity2(item, AccessControlSections.Access), includeExplicit, includeInherited, getInheritedFrom);
        }

        public static IEnumerable<FileSystemAccessRule2> GetFileSystemAccessRules(
          FileSystemSecurity2 sd,
          bool includeExplicit,
          bool includeInherited,
          bool getInheritedFrom = false)
        {
            List<FileSystemAccessRule2> systemAccessRule2List = new List<FileSystemAccessRule2>();
            List<string> stringList = (List<string>)null;
            if (getInheritedFrom)
                stringList = Win32.GetInheritedFrom(sd.Item, (ObjectSecurity)sd.SecurityDescriptor);
            int index = 0;
            foreach (FileSystemAccessRule fileSystemAccessRule in !sd.IsFile ? (ReadOnlyCollectionBase)sd.SecurityDescriptor.GetAccessRules(includeExplicit, includeInherited, typeof(SecurityIdentifier)) : (ReadOnlyCollectionBase)sd.SecurityDescriptor.GetAccessRules(includeExplicit, includeInherited, typeof(SecurityIdentifier)))
            {
                FileSystemAccessRule2 systemAccessRule2 = new FileSystemAccessRule2(fileSystemAccessRule)
                {
                    FullName = sd.Item.FullName,
                    InheritanceEnabled = !sd.SecurityDescriptor.AreAccessRulesProtected
                };
                if (getInheritedFrom)
                {
                    systemAccessRule2.inheritedFrom = string.IsNullOrEmpty(stringList[index]) ? "" : stringList[index].Substring(0, stringList[index].Length - 1);
                    ++index;
                }
                systemAccessRule2List.Add(systemAccessRule2);
            }
            return (IEnumerable<FileSystemAccessRule2>)systemAccessRule2List;
        }

        public static IEnumerable<FileSystemAccessRule2> GetFileSystemAccessRules(
          string path,
          bool includeExplicit,
          bool includeInherited,
          bool getInheritedFrom = false)
        {
            return Alphaleonis.Win32.Filesystem.File.Exists(path) ? FileSystemAccessRule2.GetFileSystemAccessRules((Alphaleonis.Win32.Filesystem.FileSystemInfo)new Alphaleonis.Win32.Filesystem.FileInfo(path), includeExplicit, includeInherited, getInheritedFrom) : FileSystemAccessRule2.GetFileSystemAccessRules((Alphaleonis.Win32.Filesystem.FileSystemInfo)new Alphaleonis.Win32.Filesystem.DirectoryInfo(path), includeExplicit, includeInherited, getInheritedFrom);
        }

        public static FileSystemAccessRule2 AddFileSystemAccessRule(
          FileSystemSecurity2 sd,
          IdentityReference2 account,
          FileSystemRights2 rights,
          AccessControlType type,
          InheritanceFlags inheritanceFlags,
          PropagationFlags propagationFlags)
        {
            if (type == AccessControlType.Allow)
                rights |= FileSystemRights2.Synchronize;
            FileSystemAccessRule rule;
            if (sd.IsFile)
            {
                rule = (FileSystemAccessRule)sd.SecurityDescriptor.AccessRuleFactory((IdentityReference)account, (int)rights, false, InheritanceFlags.None, PropagationFlags.None, type);
                sd.SecurityDescriptor.AddAccessRule(rule);
            }
            else
            {
                rule = (FileSystemAccessRule)sd.SecurityDescriptor.AccessRuleFactory((IdentityReference)account, (int)rights, false, inheritanceFlags, propagationFlags, type);
                sd.SecurityDescriptor.AddAccessRule(rule);
            }
            return (FileSystemAccessRule2)rule;
        }

        public static FileSystemAccessRule2 AddFileSystemAccessRule(
          Alphaleonis.Win32.Filesystem.FileSystemInfo item,
          IdentityReference2 account,
          FileSystemRights2 rights,
          AccessControlType type,
          InheritanceFlags inheritanceFlags,
          PropagationFlags propagationFlags)
        {
            if (type == AccessControlType.Allow)
                rights |= FileSystemRights2.Synchronize;
            FileSystemSecurity2 sd = new FileSystemSecurity2(item);
            FileSystemAccessRule2 systemAccessRule2 = FileSystemAccessRule2.AddFileSystemAccessRule(sd, account, rights, type, inheritanceFlags, propagationFlags);
            sd.Write();
            return systemAccessRule2;
        }

        public static IEnumerable<FileSystemAccessRule2> AddFileSystemAccessRule(
          Alphaleonis.Win32.Filesystem.FileSystemInfo item,
          List<IdentityReference2> accounts,
          FileSystemRights2 rights,
          AccessControlType type,
          InheritanceFlags inheritanceFlags,
          PropagationFlags propagationFlags)
        {
            List<FileSystemAccessRule2> systemAccessRule2List = new List<FileSystemAccessRule2>();
            foreach (IdentityReference2 account in accounts)
                systemAccessRule2List.Add(FileSystemAccessRule2.AddFileSystemAccessRule(item, account, rights, type, inheritanceFlags, propagationFlags));
            return (IEnumerable<FileSystemAccessRule2>)systemAccessRule2List;
        }

        public static IEnumerable<FileSystemAccessRule2> AddFileSystemAccessRule(
          FileSystemSecurity2 sd,
          List<IdentityReference2> accounts,
          FileSystemRights2 rights,
          AccessControlType type,
          InheritanceFlags inheritanceFlags,
          PropagationFlags propagationFlags)
        {
            List<FileSystemAccessRule2> systemAccessRule2List = new List<FileSystemAccessRule2>();
            foreach (IdentityReference2 account in accounts)
                systemAccessRule2List.Add(FileSystemAccessRule2.AddFileSystemAccessRule(sd, account, rights, type, inheritanceFlags, propagationFlags));
            return (IEnumerable<FileSystemAccessRule2>)systemAccessRule2List;
        }

        public static FileSystemAccessRule2 AddFileSystemAccessRule(
          string path,
          IdentityReference2 account,
          FileSystemRights2 rights,
          AccessControlType type,
          InheritanceFlags inheritanceFlags,
          PropagationFlags propagationFlags)
        {
            if (type == AccessControlType.Allow)
                rights |= FileSystemRights2.Synchronize;
            return (FileSystemAccessRule2)(!Alphaleonis.Win32.Filesystem.File.Exists(path) ? (FileSystemAccessRule)FileSystemAccessRule2.AddFileSystemAccessRule((Alphaleonis.Win32.Filesystem.FileSystemInfo)new Alphaleonis.Win32.Filesystem.DirectoryInfo(path), account, rights, type, inheritanceFlags, propagationFlags) : (FileSystemAccessRule)FileSystemAccessRule2.AddFileSystemAccessRule((Alphaleonis.Win32.Filesystem.FileSystemInfo)new Alphaleonis.Win32.Filesystem.FileInfo(path), account, rights, type, inheritanceFlags, propagationFlags));
        }

        public static IEnumerable<FileSystemAccessRule2> AddFileSystemAccessRule(
          string path,
          List<IdentityReference2> accounts,
          FileSystemRights2 rights,
          AccessControlType type,
          InheritanceFlags inheritanceFlags,
          PropagationFlags propagationFlags)
        {
            if (type == AccessControlType.Allow)
                rights |= FileSystemRights2.Synchronize;
            if (Alphaleonis.Win32.Filesystem.File.Exists(path))
            {
                Alphaleonis.Win32.Filesystem.FileInfo item = new Alphaleonis.Win32.Filesystem.FileInfo(path);
                foreach (IdentityReference2 account in accounts)
                    yield return FileSystemAccessRule2.AddFileSystemAccessRule((Alphaleonis.Win32.Filesystem.FileSystemInfo)item, account, rights, type, inheritanceFlags, propagationFlags);
                item = (Alphaleonis.Win32.Filesystem.FileInfo)null;
            }
            else
            {
                Alphaleonis.Win32.Filesystem.DirectoryInfo item = new Alphaleonis.Win32.Filesystem.DirectoryInfo(path);
                foreach (IdentityReference2 account in accounts)
                    yield return FileSystemAccessRule2.AddFileSystemAccessRule((Alphaleonis.Win32.Filesystem.FileSystemInfo)item, account, rights, type, inheritanceFlags, propagationFlags);
                item = (Alphaleonis.Win32.Filesystem.DirectoryInfo)null;
            }
        }

        public static void AddFileSystemAccessRule(FileSystemAccessRule2 rule) => FileSystemAccessRule2.AddFileSystemAccessRule(rule.fullName, rule.Account, rule.AccessRights, rule.AccessControlType, rule.InheritanceFlags, rule.PropagationFlags);
    }

    internal class SafeAuthzRMHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeAuthzRMHandle()
          : base(true)
        {
        }

        private SafeAuthzRMHandle(IntPtr handle)
          : base(true)
        {
            this.SetHandle(handle);
        }

        public static SafeAuthzRMHandle InvalidHandle => new SafeAuthzRMHandle(IntPtr.Zero);

        protected override bool ReleaseHandle() => SafeAuthzRMHandle.NativeMethods.AuthzFreeResourceManager(this.handle);

        private static class NativeMethods
        {
            [SuppressUnmanagedCodeSecurity]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [DllImport("authz.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool AuthzFreeResourceManager(IntPtr handle);
        }
    }

    public class RightsConverionException : Exception
    {
        public RightsConverionException(string Message)
          : base(Message)
        {
        }
    }

    [Flags]
    internal enum FileFlagAttrib : uint
    {
        BackupSemantics = 33554432, // 0x02000000
    }

    internal enum AuthzRpcClientVersion : ushort
    {
        V1 = 1,
    }

    public class SimpleFileSystemAccessRule
    {
        private string fullName;
        private IdentityReference2 identity;
        private FileSystemRights2 accessRights;
        private AccessControlType type;

        public AccessControlType AccessControlType
        {
            get => this.type;
            set => this.type = value;
        }

        public string FullName => this.fullName;

        public string Name => Alphaleonis.Win32.Filesystem.Path.GetFileName(this.fullName);

        public IdentityReference2 Identity => this.identity;

        public SimpleFileSystemAccessRights AccessRights
        {
            get
            {
                SimpleFileSystemAccessRights systemAccessRights = SimpleFileSystemAccessRights.None;
                if ((this.accessRights & FileSystemRights2.Read) == FileSystemRights2.Read)
                    systemAccessRights |= SimpleFileSystemAccessRights.Read;
                if ((this.accessRights & FileSystemRights2.CreateFiles) == FileSystemRights2.CreateFiles)
                    systemAccessRights |= SimpleFileSystemAccessRights.Write;
                if ((this.accessRights & FileSystemRights2.CreateDirectories) == FileSystemRights2.CreateDirectories)
                    systemAccessRights |= SimpleFileSystemAccessRights.Write;
                if ((this.accessRights & FileSystemRights2.ReadExtendedAttributes) == FileSystemRights2.ReadExtendedAttributes)
                    systemAccessRights |= SimpleFileSystemAccessRights.Read;
                if ((this.accessRights & FileSystemRights2.WriteExtendedAttributes) == FileSystemRights2.WriteExtendedAttributes)
                    systemAccessRights |= SimpleFileSystemAccessRights.Write;
                if ((this.accessRights & FileSystemRights2.ExecuteFile) == FileSystemRights2.ExecuteFile)
                    systemAccessRights |= SimpleFileSystemAccessRights.Read;
                if ((this.accessRights & FileSystemRights2.DeleteSubdirectoriesAndFiles) == FileSystemRights2.DeleteSubdirectoriesAndFiles)
                    systemAccessRights |= SimpleFileSystemAccessRights.Delete;
                if ((this.accessRights & FileSystemRights2.ReadAttributes) == FileSystemRights2.ReadAttributes)
                    systemAccessRights |= SimpleFileSystemAccessRights.Read;
                if ((this.accessRights & FileSystemRights2.WriteAttributes) == FileSystemRights2.WriteAttributes)
                    systemAccessRights |= SimpleFileSystemAccessRights.Write;
                if ((this.accessRights & FileSystemRights2.Delete) == FileSystemRights2.Delete)
                    systemAccessRights |= SimpleFileSystemAccessRights.Delete;
                if ((this.accessRights & FileSystemRights2.ReadPermissions) == FileSystemRights2.ReadPermissions)
                    systemAccessRights |= SimpleFileSystemAccessRights.Read;
                if ((this.accessRights & FileSystemRights2.ChangePermissions) == FileSystemRights2.ChangePermissions)
                    systemAccessRights |= SimpleFileSystemAccessRights.Write;
                if ((this.accessRights & FileSystemRights2.TakeOwnership) == FileSystemRights2.TakeOwnership)
                    systemAccessRights |= SimpleFileSystemAccessRights.Write;
                if ((this.accessRights & FileSystemRights2.Synchronize) == FileSystemRights2.Synchronize)
                    systemAccessRights |= SimpleFileSystemAccessRights.Read;
                if ((this.accessRights & FileSystemRights2.FullControl) == FileSystemRights2.FullControl)
                    systemAccessRights = SimpleFileSystemAccessRights.Read | SimpleFileSystemAccessRights.Write | SimpleFileSystemAccessRights.Delete;
                if ((this.accessRights & FileSystemRights2.GenericRead) == FileSystemRights2.GenericRead)
                    systemAccessRights |= SimpleFileSystemAccessRights.Read;
                if ((this.accessRights & FileSystemRights2.GenericWrite) == FileSystemRights2.GenericWrite)
                    systemAccessRights |= SimpleFileSystemAccessRights.Write;
                if ((this.accessRights & FileSystemRights2.GenericExecute) == FileSystemRights2.GenericExecute)
                    systemAccessRights |= SimpleFileSystemAccessRights.Read;
                if ((this.accessRights & FileSystemRights2.GenericAll) == FileSystemRights2.GenericAll)
                    systemAccessRights = SimpleFileSystemAccessRights.Read | SimpleFileSystemAccessRights.Write | SimpleFileSystemAccessRights.Delete;
                return systemAccessRights;
            }
        }

        public SimpleFileSystemAccessRule(
          string Path,
          IdentityReference2 account,
          FileSystemRights2 access)
        {
            this.fullName = Path;
            this.accessRights = access;
            this.identity = account;
        }

        public override bool Equals(object obj) => obj is SimpleFileSystemAccessRule systemAccessRule && this.AccessRights == systemAccessRule.AccessRights && this.Identity == systemAccessRule.Identity && this.AccessControlType == systemAccessRule.AccessControlType;

        public override int GetHashCode() => this.Identity.GetHashCode() | this.AccessRights.GetHashCode() | this.AccessControlType.GetHashCode();
    }

    [Flags]
    internal enum AuthzResourceManagerFlags : uint
    {
        NO_AUDIT = 1,
    }

    [Flags]
    internal enum AuthzInitFlags : uint
    {
        Default = 0,
        SkipTokenGroups = 2,
        RequireS4ULogon = 4,
        ComputePrivileges = 8,
    }

    [Flags]
    internal enum SecurityInformationClass : uint
    {
        Owner = 1,
        Group = 2,
        Dacl = 4,
        Sacl = 8,
        Label = 16, // 0x00000010
        Attribute = 32, // 0x00000020
        Scope = 64, // 0x00000040
    }

    [Flags]
    public enum FileSystemRights2 : uint
    {
        None = 0,
        ListDirectory = 1,
        ReadData = ListDirectory, // 0x00000001
        CreateFiles = 2,
        CreateDirectories = 4,
        AppendData = CreateDirectories, // 0x00000004
        ReadExtendedAttributes = 8,
        WriteExtendedAttributes = 16, // 0x00000010
        ExecuteFile = 32, // 0x00000020
        Traverse = ExecuteFile, // 0x00000020
        DeleteSubdirectoriesAndFiles = 64, // 0x00000040
        ReadAttributes = 128, // 0x00000080
        WriteAttributes = 256, // 0x00000100
        Write = WriteAttributes | WriteExtendedAttributes | AppendData | CreateFiles, // 0x00000116
        Delete = 65536, // 0x00010000
        ReadPermissions = 131072, // 0x00020000
        Read = ReadPermissions | ReadAttributes | ReadExtendedAttributes | ReadData, // 0x00020089
        ReadAndExecute = Read | Traverse, // 0x000200A9
        Modify = ReadAndExecute | Delete | Write, // 0x000301BF
        ChangePermissions = 262144, // 0x00040000
        TakeOwnership = 524288, // 0x00080000
        Synchronize = 1048576, // 0x00100000
        FullControl = Synchronize | TakeOwnership | ChangePermissions | Modify | DeleteSubdirectoriesAndFiles, // 0x001F01FF
        GenericRead = 2147483648, // 0x80000000
        GenericWrite = 1073741824, // 0x40000000
        GenericExecute = 536870912, // 0x20000000
        GenericAll = 268435456, // 0x10000000
    }

    [Flags]
    public enum SimpleFileSystemAccessRights
    {
        None = 0,
        Read = 1,
        Write = 2,
        Delete = 4,
    }

    internal enum SECURITY_INFORMATION
    {
        OWNER_SECURITY_INFORMATION = 1,
        GROUP_SECURITY_INFORMATION = 2,
        DACL_SECURITY_INFORMATION = 4,
        SACL_SECURITY_INFORMATION = 8,
    }

    [Flags]
    internal enum StdAccess : uint
    {
        None = 0,
        SYNCHRONIZE = 1048576, // 0x00100000
        STANDARD_RIGHTS_REQUIRED = 983040, // 0x000F0000
        MAXIMUM_ALLOWED = 33554432, // 0x02000000
    }

    internal enum ObjectType : uint
    {
        File = 1,
    }

    public enum ApplyTo
    {
        ThisFolderOnly,
        ThisFolderSubfoldersAndFiles,
        ThisFolderAndSubfolders,
        ThisFolderAndFiles,
        SubfoldersAndFilesOnly,
        SubfoldersOnly,
        FilesOnly,
        ThisFolderSubfoldersAndFilesOneLevel,
        ThisFolderAndSubfoldersOneLevel,
        ThisFolderAndFilesOneLevel,
        SubfoldersAndFilesOnlyOneLevel,
        SubfoldersOnlyOneLevel,
        FilesOnlyOneLevel,
    }

    internal sealed class SafeHGlobalHandle : IDisposable
    {
        private List<SafeHGlobalHandle> references;
        private IntPtr pointer;

        private SafeHGlobalHandle() => this.pointer = IntPtr.Zero;

        private SafeHGlobalHandle(IntPtr handle) => this.pointer = handle;

        ~SafeHGlobalHandle() => this.Dispose();

        public static SafeHGlobalHandle InvalidHandle => new SafeHGlobalHandle(IntPtr.Zero);

        public void AddSubReference(IEnumerable<SafeHGlobalHandle> children)
        {
            if (this.references == null)
                this.references = new List<SafeHGlobalHandle>();
            this.references.AddRange(children);
        }

        public static SafeHGlobalHandle AllocHGlobal(IntPtr[] values)
        {
            SafeHGlobalHandle safeHglobalHandle = SafeHGlobalHandle.AllocHGlobal(IntPtr.Size * values.Length);
            Marshal.Copy(values, 0, safeHglobalHandle.pointer, values.Length);
            return safeHglobalHandle;
        }

        public static SafeHGlobalHandle AllocHGlobalStruct<T>(T obj) where T : struct
        {
            SafeHGlobalHandle safeHglobalHandle = SafeHGlobalHandle.AllocHGlobal(Marshal.SizeOf(typeof(T)));
            Marshal.StructureToPtr((object)obj, safeHglobalHandle.pointer, false);
            return safeHglobalHandle;
        }

        public static SafeHGlobalHandle AllocHGlobal<T>(ICollection<T> values) where T : struct => SafeHGlobalHandle.AllocHGlobal<T>(0, (IEnumerable<T>)values, values.Count);

        public static SafeHGlobalHandle AllocHGlobal<T>(
          int prefixBytes,
          IEnumerable<T> values,
          int count)
          where T : struct
        {
            SafeHGlobalHandle safeHglobalHandle = SafeHGlobalHandle.AllocHGlobal(prefixBytes + Marshal.SizeOf(typeof(T)) * count);
            IntPtr ptr = new IntPtr(safeHglobalHandle.pointer.ToInt32() + prefixBytes);
            foreach (T obj in values)
            {
                Marshal.StructureToPtr((object)obj, ptr, false);
                ptr.Increment<T>();
            }
            return safeHglobalHandle;
        }

        public static SafeHGlobalHandle AllocHGlobal(string s) => new SafeHGlobalHandle(Marshal.StringToHGlobalUni(s));

        public IntPtr ToIntPtr() => this.pointer;

        public void Dispose()
        {
            if (this.pointer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(this.pointer);
                this.pointer = IntPtr.Zero;
            }
            GC.SuppressFinalize((object)this);
        }

        private static SafeHGlobalHandle AllocHGlobal(int cb)
        {
            if (cb < 0)
                throw new ArgumentOutOfRangeException(nameof(cb), "The value of this argument must be non-negative");
            SafeHGlobalHandle safeHglobalHandle = new SafeHGlobalHandle();
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                safeHglobalHandle.pointer = Marshal.AllocHGlobal(cb);
            }
            return safeHglobalHandle;
        }
    }
    internal static class IntPtrExtensions
    {
        public static IntPtr Increment(this IntPtr ptr, int cbSize) => new IntPtr(ptr.ToInt64() + (long)cbSize);

        public static IntPtr Increment<T>(this IntPtr ptr) => ptr.Increment(Marshal.SizeOf(typeof(T)));

        public static T ElementAt<T>(this IntPtr ptr, int index)
        {
            int cbSize = Marshal.SizeOf(typeof(T)) * index;
            return (T)Marshal.PtrToStructure(ptr.Increment(cbSize), typeof(T));
        }
    }
}
