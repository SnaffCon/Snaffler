using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace Classifiers
{
    public partial class Classifier
    {
        // Methods for classification
        internal bool SimpleMatch(string input)
        {
            // generic match checking
            switch (this.WordListType)
            {
                case MatchListType.Contains:
                    foreach (string matchString in this.WordList)
                    {
                        if (input.Contains(matchString))
                        {
                            return true;
                        }
                    }

                    break;
                case MatchListType.EndsWith:
                    foreach (string matchString in this.WordList)
                    {
                        if (input.EndsWith(matchString))
                        {
                            return true;
                        }
                    }

                    break;
                case MatchListType.Exact:
                    foreach (string matchString in this.WordList)
                    {
                        if (input == matchString)
                        {
                            return true;
                        }
                    }

                    break;
                case MatchListType.StartsWith:
                    foreach (string matchString in this.WordList)
                    {
                        if (input.StartsWith(matchString))
                        {
                            return true;
                        }
                    }

                    break;
                case MatchListType.Regex:
                    foreach (string matchString in this.WordList)
                    {
                        Regex regex = new Regex(matchString);
                        if (regex.IsMatch(input))
                        {
                            return true;
                        }
                    }

                    break;
                default:
                    return false;
            }

            return false;
        }

        internal bool x509PrivKeyMatch(FileInfo fileInfo)
        {
            try
            {
                X509Certificate2 parsedCert = new X509Certificate2(fileInfo.FullName);
                if (parsedCert.HasPrivateKey) return true;
            }
            catch (CryptographicException)
            {
                return false;
            }

            return false;
        }

        private RwStatus CanRw(FileInfo fileInfo)
        {
            try
            {
                RwStatus rwStatus = new RwStatus { CanWrite = CanIWrite(fileInfo), CanRead = CanIRead(fileInfo) };
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
                        GreppedStrings = new List<string> { funString }
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
    }
}