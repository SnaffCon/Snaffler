using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text.RegularExpressions;
using SnaffCore.Config;
using SnaffCore.TreeWalk;

namespace Classifiers
{
    public partial class Classifier
    {
        // define in what phase this classifier is run
        public EnumerationScope EnumerationScope { get; set; } = EnumerationScope.FileEnumeration;

        // define a way to chain classifiers together
        public string ClassifierName { get; set; } = "Default";
        public MatchAction MatchAction { get; set; } = MatchAction.Snaffle;
        public string RelayTarget { get; set; } = null;

        // define the behaviour of this classifier
        public MatchLoc MatchLocation { get; set; } = MatchLoc.FileName;
        public MatchListType WordListType { get; set; } = MatchListType.Contains;
        public List<string> WordList { get; set; } = new List<string>();

        // define the severity of this classification
        public Triage Triage { get; set; } = Triage.Black;
    }

    public enum EnumerationScope
    {
        ShareEnumeration,
        DirectoryEnumeration,
        FileEnumeration,
        ContentsEnumeration
    }

    public enum MatchLoc
    {
        ShareName,
        FilePath,
        FileName,
        FileExtension,
        FileContentAsString,
        FileContentAsBytes
    }

    public enum MatchListType
    {
        Exact,
        Contains,
        Regex,
        EndsWith,
        StartsWith
    }

    public enum MatchAction
    {
        Discard,
        SendToNextScope,
        Snaffle,
        Relay,
        CheckForKeys,
        EnterArchive
    }

    public enum Triage
    {
        Black,
        Green,
        Yellow,
        Red
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