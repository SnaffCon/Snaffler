using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace SnaffCore.ShareScan
{
    public class FileScanner
    {
        public class FileResult
        {
            public FileInfo FileInfo { get; set; }
            public GrepFileResult GrepFileResult { get; set; }
            public RwStatus RwStatus { get; set; }
            public MatchReason WhyMatched { get; set; } = MatchReason.NoMatch;
        }

        public class GrepFileResult
        {
            public List<string> GreppedStrings { get; set; }
            public string GrepContext { get; set; }
        }

        public enum MatchReason
        {
            NoMatch,
            ExactExtensionMatch,
            ExactFileNameMatch,
            PartialPathMatch,
            PartialFileNameMatch,
            FileContainsInterestingStrings
        }

        public class RwStatus
        {
            public bool CanRead { get; set; }
            public bool CanWrite { get; set; }
        }

        private RwStatus CanRw(FileInfo fileInfo)
        {
            try
            {
                var rwStatus = new RwStatus {CanWrite = CanIWrite(fileInfo), CanRead = CanIRead(fileInfo)};
                return rwStatus;
            }
            catch
            {
                return null;
            }
        }

        public static bool CanIRead(FileInfo fileInfo)
        {
            // this will return true if file read perm is available.
            var currentUserSecurity = new CurrentUserSecurity();

            FileSystemRights[] fsRights =
            {
                FileSystemRights.Read,
                FileSystemRights.ReadAndExecute,
                FileSystemRights.ReadData
            };

            var readRight = false;
            foreach (var fsRight in fsRights)
            {
                try
                {
                    if (currentUserSecurity.HasAccess(fileInfo, fsRight))
                    {
                        readRight = true;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    return false;
                }
            }

            return readRight;
        }

        public static bool CanIWrite(FileInfo fileInfo)
        {
            // this will return true if write or modify or take ownership or any of those other good perms are available.
            var currentUserSecurity = new CurrentUserSecurity();

            FileSystemRights[] fsRights =
            {
                FileSystemRights.Write,
                FileSystemRights.Modify,
                FileSystemRights.FullControl,
                FileSystemRights.TakeOwnership,
                FileSystemRights.ChangePermissions,
                FileSystemRights.AppendData,
                FileSystemRights.WriteData
            };

            var writeRight = false;
            foreach (var fsRight in fsRights)
            {
                try
                {
                    if (currentUserSecurity.HasAccess(fileInfo, fsRight))
                    {
                        writeRight = true;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    return false;
                }
            }

            return writeRight;
        }

        // checks a file to see if it's cool or not.
        public FileResult Scan(FileInfo fileInfo, Config.Config config)
        {
            // if each check is enabled in FileScannerConfig, run it on the thing.
            if (config.ExactExtensionSkipCheck)
            {
                if (ExactExtCheck(fileInfo, config.ExtSkipList))
                {
                    return null;
                }
            }

            if (config.PartialPathCheck)
            {
                if (PartialPathCheck(fileInfo, config.PathsToKeep))
                {
                    var rwStatus = CanRw(fileInfo);
                    if (rwStatus.CanRead || rwStatus.CanWrite)
                    {
                        return new FileResult {FileInfo = fileInfo, WhyMatched = MatchReason.PartialPathMatch, RwStatus = rwStatus};
                    }
                }
            }

            if (config.ExactNameCheck)
            {
                if (ExactNameCheck(fileInfo, config.FileNamesToKeep))
                {
                    var rwStatus = CanRw(fileInfo);
                    if (rwStatus.CanRead || rwStatus.CanWrite)
                    {
                        return new FileResult {FileInfo = fileInfo, WhyMatched = MatchReason.ExactFileNameMatch, RwStatus = rwStatus};
                    }
                }
            }

            if (config.ExactExtensionCheck)
            {
                if (ExactExtCheck(fileInfo, config.ExtensionsToKeep))
                {
                    var rwStatus = CanRw(fileInfo);
                    if (rwStatus.CanRead || rwStatus.CanWrite)
                    {
                        return new FileResult {FileInfo = fileInfo, WhyMatched = MatchReason.ExactExtensionMatch, RwStatus = rwStatus};
                    }
                }
            }

            if (config.PartialNameCheck)
            {
                if (PartialNameCheck(fileInfo, config.NameStringsToKeep))
                {
                    var rwStatus = CanRw(fileInfo);
                    if (rwStatus.CanRead || rwStatus.CanWrite)
                    {
                        return new FileResult {FileInfo = fileInfo, WhyMatched = MatchReason.PartialFileNameMatch, RwStatus = rwStatus};
                    }
                }
            }

            if (config.GrepByExtensionCheck)
            {
                // this is for later when i try actually parsing these suckers.
                // var x509 = new X509Certificate2(File.ReadAllBytes(_path));

                if (ExactExtCheck(fileInfo, config.ExtensionsToGrep))
                {
                    if (fileInfo.Length < config.MaxSizeToGrep)
                    {
                        var grepFileResult = GrepFile(fileInfo, config.GrepStrings, config.GrepContextBytes);

                        if (grepFileResult != null)
                        {
                            var rwStatus = CanRw(fileInfo);

                            if (rwStatus.CanRead || rwStatus.CanWrite)
                            {
                                return new FileResult
                                {
                                    FileInfo = fileInfo,
                                    WhyMatched = MatchReason.FileContainsInterestingStrings,
                                    RwStatus = rwStatus,
                                    GrepFileResult = grepFileResult
                                };
                            }
                        }
                    }
                }
            }

            return null;
        }

        internal bool PartialNameCheck(FileInfo fileInfo, string[] nameStringsToKeep)
        {
            var fileResult = new FileResult();

            if (PartialMatchInArray(fileInfo.Name, nameStringsToKeep))
            {
                return true;
            }

            return false;
        }

        internal bool PartialPathCheck(FileInfo fileInfo, string[] pathsToKeep)
        {
            var fileResult = new FileResult();

            if (PartialMatchInArray(fileInfo.FullName, pathsToKeep))
            {
                return true;
            }

            return false;
        }

        internal bool ExactNameCheck(FileInfo fileInfo, string[] fileNamesToKeep)
        {
            if (ExactMatchInArray(fileInfo.Name, fileNamesToKeep))
            {
                return true;
            }

            return false;
        }

        internal bool ExactExtCheck(FileInfo fileInfo, string[] extensionsToKeep)
        {
            if (ExactMatchInArray(fileInfo.Extension, extensionsToKeep))
            {
                return true;
            }

            return false;
        }

        internal bool ExactMatchInArray(string inString, string[] inArr)
        {
            // finds if inString matches any of the strings in inArr, case-insensitive.
            var matched = false;
            foreach (var arrString in inArr)
            {
                if (inString.Equals(arrString, StringComparison.OrdinalIgnoreCase))
                {
                    matched = true;
                    break;
                }
            }

            return matched;
        }

        internal bool PartialMatchInArray(string inString, string[] inArr)
        {
            // finds if inString contains any of the strings in inArr, case-insensitive.
            var matched = false;
            foreach (var arrString in inArr)
            {
                if (inString.IndexOf(arrString, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    matched = true;
                    break;
                }
            }

            return matched;
        }

        internal GrepFileResult GrepFile(FileInfo fileInfo, string[] grepStrings, int contextBytes)
        {
            var foundStrings = new List<string>();

            var fileContents = File.ReadAllText(fileInfo.FullName);

            foreach (var funString in grepStrings)
            {
                var foundIndex = fileContents.IndexOf(funString, StringComparison.OrdinalIgnoreCase);

                if (foundIndex >= 0)
                {
                    var contextStart = SubtractWithFloor(foundIndex, contextBytes, 0);
                    var grepContext = "";
                    if (contextBytes > 0)
                    {
                        grepContext = fileContents.Substring(contextStart, (contextBytes * 2));
                    }

                    return new GrepFileResult
                    {
                        GrepContext = Regex.Escape(grepContext),
                        GreppedStrings = new List<string> {funString}
                    };
                }
            }

            return null;
        }

        internal int SubtractWithFloor(int num1, int num2, int floor)
        {
            var result = num1 - num2;
            if (result <= floor) return floor;
            return result;
        }

        public class CurrentUserSecurity
        {
            private WindowsIdentity _currentUser;
            private WindowsPrincipal _currentPrincipal;

            public CurrentUserSecurity()
            {
                _currentUser = WindowsIdentity.GetCurrent();
                _currentPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            }

            public bool HasAccess(DirectoryInfo directory, FileSystemRights right)
            {
                try
                {
                    // Get the collection of authorization rules that apply to the directory.
                    var acl = directory.GetAccessControl()
                        .GetAccessRules(true, true, typeof(SecurityIdentifier));
                    return HasFileOrDirectoryAccess(right, acl);
                }

                catch (UnauthorizedAccessException)
                {
                    return false;
                }
            }

            public bool HasAccess(FileInfo file, FileSystemRights right)
            {
                try
                {
                    // Get the collection of authorization rules that apply to the file.
                    var acl = file.GetAccessControl()
                        .GetAccessRules(true, true, typeof(SecurityIdentifier));

                    return HasFileOrDirectoryAccess(right, acl);
                }

                catch (UnauthorizedAccessException)
                {
                    return false;
                }
            }

            private bool HasFileOrDirectoryAccess(FileSystemRights right,
                AuthorizationRuleCollection acl)
            {
                var allow = false;
                var inheritedAllow = false;
                var inheritedDeny = false;

                for (var i = 0; i < acl.Count; i++)
                {
                    var currentRule = (FileSystemAccessRule) acl[i];
                    // If the current rule applies to the current user.
                    if (_currentUser.User.Equals(currentRule.IdentityReference) ||
                        _currentPrincipal.IsInRole(
                            (SecurityIdentifier) currentRule.IdentityReference))
                    {
                        if (currentRule.AccessControlType.Equals(AccessControlType.Deny))
                        {
                            if ((currentRule.FileSystemRights & right) == right)
                            {
                                if (currentRule.IsInherited)
                                {
                                    inheritedDeny = true;
                                }
                                else
                                {
                                    // Non inherited "deny" takes overall precedence.
                                    return false;
                                }
                            }
                        }
                        else if (currentRule.AccessControlType
                            .Equals(AccessControlType.Allow))
                        {
                            if ((currentRule.FileSystemRights & right) == right)
                            {
                                if (currentRule.IsInherited)
                                {
                                    inheritedAllow = true;
                                }
                                else
                                {
                                    allow = true;
                                }
                            }
                        }
                    }
                }

                if (allow)
                {
                    // Non inherited "allow" takes precedence over inherited rules.
                    return true;
                }

                return inheritedAllow && !inheritedDeny;
            }
        }
    }
}