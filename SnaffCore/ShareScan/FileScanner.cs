using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text.RegularExpressions;
using SnaffCore.Concurrency;
using Config = SnaffCore.Config.Config;


namespace SnaffCore.ShareScan
{
    public class FileScanner
    {
        public enum MatchReason
        {
            NoMatch,
            ExactExtensionMatch,
            ExactFileNameMatch,
            PartialPathMatch,
            PartialFileNameMatch,
            FileContainsInterestingStrings
        }

        private RwStatus CanRw(FileInfo fileInfo)
        {
            try
            {
                RwStatus rwStatus = new RwStatus {CanWrite = CanIWrite(fileInfo), CanRead = CanIRead(fileInfo)};
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
            CurrentUserSecurity currentUserSecurity = new CurrentUserSecurity();

            FileSystemRights[] fsRights =
            {
                FileSystemRights.Read,
                FileSystemRights.ReadAndExecute,
                FileSystemRights.ReadData
            };

            bool readRight = false;
            foreach (FileSystemRights fsRight in fsRights)
                try
                {
                    if (currentUserSecurity.HasAccess(fileInfo, fsRight)) readRight = true;
                }
                catch (UnauthorizedAccessException)
                {
                    return false;
                }

            return readRight;
        }

        public static bool CanIWrite(FileInfo fileInfo)
        {
            // this will return true if write or modify or take ownership or any of those other good perms are available.
            CurrentUserSecurity currentUserSecurity = new CurrentUserSecurity();

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

            bool writeRight = false;
            foreach (FileSystemRights fsRight in fsRights)
                try
                {
                    if (currentUserSecurity.HasAccess(fileInfo, fsRight)) writeRight = true;
                }
                catch (UnauthorizedAccessException)
                {
                    return false;
                }

            return writeRight;
        }

        // checks a file to see if it's cool or not.
        public FileResult ApplyRuleset(FileInfo fileInfo)
        {
            // TODO: implement the Rulesets for a file path
            return null;
        }
        
        
        public FileResult Scan(FileInfo fileInfo)
        {
            BlockingMq Mq = BlockingMq.GetMq();
            Config.Config myConfig = Config.Config.GetConfig();
            // if each check is enabled in FileScannerConfig, run it on the thing.

            if (myConfig.Options.ExactExtensionSkipCheck)
                if (ExactExtCheck(fileInfo, myConfig.Options.DiscardExtExact))
                    return null;

            if (myConfig.Options.PartialPathCheck)
                if (PartialPathCheck(fileInfo, myConfig.Options.KeepFilepathContains))
                {
                    RwStatus rwStatus = CanRw(fileInfo);
                    if (rwStatus.CanRead || rwStatus.CanWrite)
                        return new FileResult
                            {FileInfo = fileInfo, WhyMatched = MatchReason.PartialPathMatch, RwStatus = rwStatus};
                }

            if (myConfig.Options.ExactNameCheck)
                if (ExactNameCheck(fileInfo, myConfig.Options.KeepFilenameExact))
                {
                    RwStatus rwStatus = CanRw(fileInfo);
                    if (rwStatus.CanRead || rwStatus.CanWrite)
                        return new FileResult
                            {FileInfo = fileInfo, WhyMatched = MatchReason.ExactFileNameMatch, RwStatus = rwStatus};
                }

            if (myConfig.Options.ExactExtensionCheck)
                if (ExactExtCheck(fileInfo, myConfig.Options.KeepExtExact))
                {
                    RwStatus rwStatus = CanRw(fileInfo);
                    if (rwStatus.CanRead || rwStatus.CanWrite)
                        return new FileResult
                            {FileInfo = fileInfo, WhyMatched = MatchReason.ExactExtensionMatch, RwStatus = rwStatus};
                }

            if (myConfig.Options.PartialNameCheck)
                if (PartialNameCheck(fileInfo, myConfig.Options.NameStringsToKeep))
                {
                    RwStatus rwStatus = CanRw(fileInfo);
                    if (rwStatus.CanRead || rwStatus.CanWrite)
                        return new FileResult
                            {FileInfo = fileInfo, WhyMatched = MatchReason.PartialFileNameMatch, RwStatus = rwStatus};
                }

            if (myConfig.Options.GrepByExtensionCheck)
                if (ExactExtCheck(fileInfo, myConfig.Options.GrepExtExact))
                    if (fileInfo.Length < myConfig.Options.MaxSizeToGrep)
                    {
                        GrepFileResult grepFileResult = GrepFile(fileInfo, myConfig.Options.GrepStrings, myConfig.Options.GrepContextBytes);

                        if (grepFileResult != null)
                        {
                            RwStatus rwStatus = CanRw(fileInfo);

                            if (rwStatus.CanRead || rwStatus.CanWrite)
                                return new FileResult
                                {
                                    FileInfo = fileInfo,
                                    WhyMatched = MatchReason.FileContainsInterestingStrings,
                                    RwStatus = rwStatus,
                                    GrepFileResult = grepFileResult
                                };
                        }
                    }

            return null;
        }

        internal bool x509PrivKeyCheck(FileInfo fileInfo)
        {
            try
            {
                X509Certificate2 parsedCert = new X509Certificate2(fileInfo.FullName);
                if (parsedCert.HasPrivateKey) return true;
            }
            catch (CryptographicException e)
            {
                return false;
            }

            return false;
        }

        internal bool regexContentCheck(FileInfo fileInfo, Regex[] regexen)
        {
            string fileContents = File.ReadAllText(fileInfo.FullName);
            if (RegexInArray(fileContents, regexen)) return true;
            return false;
        }
        internal bool regexNameCheck(FileInfo fileInfo, Regex[] regexen)
        {
            if (RegexInArray(fileInfo.FullName, regexen)) return true;
            return false;
        }

        internal bool PartialNameCheck(FileInfo fileInfo, IEnumerable<string> nameStringsToKeep)
        {
            if (PartialMatchInArray(fileInfo.Name, nameStringsToKeep)) return true;
            return false;
        }

        internal bool PartialPathCheck(FileInfo fileInfo, IEnumerable<string> pathsToKeep)
        {
            if (PartialMatchInArray(fileInfo.FullName, pathsToKeep)) return true;
            return false;
        }

        internal bool ExactNameCheck(FileInfo fileInfo, IEnumerable<string> fileNamesToKeep)
        {
            if (ExactMatchInArray(fileInfo.Name, fileNamesToKeep)) return true;
            return false;
        }

        internal bool ExactExtCheck(FileInfo fileInfo, IEnumerable<string> extensionsToKeep)
        {
            if (ExactMatchInArray(fileInfo.Extension, extensionsToKeep)) return true;
            return false;
        }

        internal bool RegexInArray(string inString, Regex[] regexen)
        {
            foreach (Regex regex in regexen)
                if (regex.Match(inString).Success)
                {
                    return true;
                }
            return false;
        }

        internal bool ExactMatchInArray(string inString, IEnumerable<string> inArr)
        {
            // finds if inString matches any of the strings in inArr, case-insensitive.
            foreach (string arrString in inArr)
                if (inString.Equals(arrString, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

            return false;
        }

        internal bool PartialMatchInArray(string inString, IEnumerable<string> inArr)
        {
            // finds if inString contains any of the strings in inArr, case-insensitive.
            bool matched = false;
            foreach (string arrString in inArr)
                if (inString.IndexOf(arrString, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    matched = true;
                    break;
                }

            return matched;
        }

        internal GrepFileResult GrepFile(FileInfo fileInfo, IEnumerable<string> grepStrings, int contextBytes)
        {
            List<string> foundStrings = new List<string>();

            string fileContents = File.ReadAllText(fileInfo.FullName);

            foreach (string funString in grepStrings)
            {
                int foundIndex = fileContents.IndexOf(funString, StringComparison.OrdinalIgnoreCase);

                if (foundIndex >= 0)
                {
                    int contextStart = SubtractWithFloor(foundIndex, contextBytes, 0);
                    string grepContext = "";
                    if (contextBytes > 0) grepContext = fileContents.Substring(contextStart, contextBytes * 2);

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
            int result = num1 - num2;
            if (result <= floor) return floor;
            return result;
        }

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

        public class RwStatus
        {
            public bool CanRead { get; set; }
            public bool CanWrite { get; set; }
        }

        public class CurrentUserSecurity
        {
            private readonly WindowsPrincipal _currentPrincipal;
            private readonly WindowsIdentity _currentUser;

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
                    AuthorizationRuleCollection acl = directory.GetAccessControl()
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
                    AuthorizationRuleCollection acl = file.GetAccessControl()
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
                bool allow = false;
                bool inheritedAllow = false;
                bool inheritedDeny = false;

                for (int i = 0; i < acl.Count; i++)
                {
                    FileSystemAccessRule currentRule = (FileSystemAccessRule) acl[i];
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
                                    inheritedDeny = true;
                                else
                                    // Non inherited "deny" takes overall precedence.
                                    return false;
                            }
                        }
                        else if (currentRule.AccessControlType
                            .Equals(AccessControlType.Allow))
                        {
                            if ((currentRule.FileSystemRights & right) == right)
                            {
                                if (currentRule.IsInherited)
                                    inheritedAllow = true;
                                else
                                    allow = true;
                            }
                        }
                    }
                }

                if (allow)
                    // Non inherited "allow" takes precedence over inherited rules.
                    return true;

                return inheritedAllow && !inheritedDeny;
            }
        }
    }
}