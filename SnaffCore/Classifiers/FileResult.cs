using System;
using System.IO;
using SnaffCore.Classifiers.EffectiveAccess;
using static SnaffCore.Config.Options;

namespace SnaffCore.Classifiers
{
    public class FileResult
    {
        public FileInfo FileInfo { get; set; }
        public TextResult TextResult { get; set; }
        public RwStatus RwStatus { get; set; }
        public ClassifierRule MatchedRule { get; set; }

        public FileResult(FileInfo fileInfo)
        {
            //EffectivePermissions effPerms = new EffectivePermissions(MyOptions.CurrentUser);

            // get an aggressively simplified version of the file's ACL
            //this.RwStatus = effPerms.CanRw(fileInfo);
            try
            {
                File.OpenRead(fileInfo.FullName);
                this.RwStatus = new RwStatus() { CanRead = true, CanModify = false, CanWrite = false };
            }
            catch (Exception e)
            {
                this.RwStatus = new RwStatus() { CanModify = false, CanRead = false, CanWrite = false };
            }

            // nasty debug
            this.FileInfo = fileInfo;

            // this is where we actually automatically grab a copy of the file if wanted.
            if (MyOptions.Snaffle)
            {
                if ((MyOptions.MaxSizeToSnaffle >= fileInfo.Length) && RwStatus.CanRead)
                {
                    SnaffleFile(fileInfo, MyOptions.SnafflePath);
                }
            }
        }

        public void SnaffleFile(FileInfo fileInfo, string snafflePath)
        {
            string sourcePath = fileInfo.FullName;
            // clean it up and normalise it a bit
            string cleanedPath = sourcePath.Replace(':', '.').Replace('$', '.').Replace("\\\\", "\\");
            //string cleanedPath = Path.GetFullPath(sourcePath.Replace(':', '.').Replace('$', '.'));
            // make the dir exist
            string snaffleFilePath = Path.Combine(snafflePath, cleanedPath);
            string snaffleDirPath = Path.GetDirectoryName(snaffleFilePath);
            Directory.CreateDirectory(snaffleDirPath);
            File.Copy(sourcePath, (Path.Combine(snafflePath, cleanedPath)), true);
        }

        /*
        public static EffectivePermissions.RwStatus CanRw(FileInfo fileInfo)
        {
            BlockingMq Mq = BlockingMq.GetMq();

            try
            {
                EffectivePermissions.RwStatus rwStatus = new EffectivePermissions.RwStatus { CanWrite = false, CanRead = false, CanModify = false };
                EffectivePermissions effPerms = new EffectivePermissions();
                string dir = fileInfo.DirectoryName;

                // we hard code this otherwise it tries to do some madness where it uses RPC with a share server to check file access, then fails if you're not admin on that host.
                string hostname = "localhost";

                string whoami = WindowsIdentity.GetCurrent().Name;

                string[] accessStrings = effPerms.GetEffectivePermissions(fileInfo, whoami);

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
                    if (readRights.Contains(access)){
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
                Mq.Error(e.ToString());
                return new EffectivePermissions.RwStatus { CanWrite = false, CanRead = false }; ;
            }
        }
        */
    }
}