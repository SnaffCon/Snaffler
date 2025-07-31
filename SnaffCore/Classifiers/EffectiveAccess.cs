

using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace SnaffCore.Classifiers.EffectiveAccess
{
    public class RwStatus
    {
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanModify { get; set; }

        public override string ToString()
        {
            char[] rwChars = { '-', '-', '-' };

            if (CanRead) rwChars[0] = 'R';
            if (CanWrite) rwChars[1] = 'W';
            if (CanModify) rwChars[2] = 'M';

            return new string(rwChars);
        }
    }

    public class EffectivePermissions
    {
        private readonly string _username;
        
        public EffectivePermissions(string username)
        {
            _username = username;
        }

        public RwStatus CanRw(AuthorizationRuleCollection acl)
        {
            RwStatus rwStatus = new RwStatus();

            try
            {
                foreach (FileSystemAccessRule rule in acl)
                {
                    if (rule.IdentityReference.Value.Equals(_username, StringComparison.OrdinalIgnoreCase))
                    {
                        if ((rwStatus.CanRead != true) && (((rule.FileSystemRights & FileSystemRights.Read) == FileSystemRights.Read) ||
                            ((rule.FileSystemRights & FileSystemRights.ReadAndExecute) == FileSystemRights.ReadAndExecute) ||
                            ((rule.FileSystemRights & FileSystemRights.ReadData) == FileSystemRights.ReadData) ||
                            ((rule.FileSystemRights & FileSystemRights.ListDirectory) == FileSystemRights.ListDirectory)))
                        {
                            rwStatus.CanRead = true;
                        }
                        if ((rwStatus.CanWrite != true) && (((rule.FileSystemRights & FileSystemRights.Write) == FileSystemRights.Write) ||
                            ((rule.FileSystemRights & FileSystemRights.Modify) == FileSystemRights.Modify) ||
                            ((rule.FileSystemRights & FileSystemRights.FullControl) == FileSystemRights.FullControl) ||
                            ((rule.FileSystemRights & FileSystemRights.TakeOwnership) == FileSystemRights.TakeOwnership) ||
                            ((rule.FileSystemRights & FileSystemRights.ChangePermissions) == FileSystemRights.ChangePermissions) ||
                            ((rule.FileSystemRights & FileSystemRights.AppendData) == FileSystemRights.AppendData) ||
                            ((rule.FileSystemRights & FileSystemRights.WriteData) == FileSystemRights.WriteData) ||
                            ((rule.FileSystemRights & FileSystemRights.CreateFiles) == FileSystemRights.CreateFiles) ||
                            ((rule.FileSystemRights & FileSystemRights.CreateDirectories) == FileSystemRights.CreateDirectories)))
                        {
                            rwStatus.CanWrite = true;
                        }
                        if ((rwStatus.CanModify != true) && (((rule.FileSystemRights & FileSystemRights.Modify) == FileSystemRights.Modify) ||
                            ((rule.FileSystemRights & FileSystemRights.FullControl) == FileSystemRights.FullControl) ||
                            ((rule.FileSystemRights & FileSystemRights.TakeOwnership) == FileSystemRights.TakeOwnership) ||
                            ((rule.FileSystemRights & FileSystemRights.ChangePermissions) == FileSystemRights.ChangePermissions)))
                        {
                            rwStatus.CanModify = true;
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException) { }

            return rwStatus;
        }

        public RwStatus CanRw(FileInfo fileInfo)
        {
            FileSecurity fileSecurity = fileInfo.GetAccessControl();
            AuthorizationRuleCollection acl = fileSecurity.GetAccessRules(true, true, typeof(NTAccount));
            return CanRw(acl);
        }

        public RwStatus CanRw(DirectoryInfo dirInfo)
        {
            DirectorySecurity dirSecurity = dirInfo.GetAccessControl();
            AuthorizationRuleCollection acl = dirSecurity.GetAccessRules(true, true, typeof(NTAccount));
            return CanRw(acl);
        }
    }
}
