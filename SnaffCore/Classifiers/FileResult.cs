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
            EffectivePermissions effPerms = new EffectivePermissions(MyOptions.CurrentUser);
            this.RwStatus = effPerms.CanRw(fileInfo);

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
            cleanedPath = cleanedPath.TrimStart('\\');
            //string cleanedPath = Path.GetFullPath(sourcePath.Replace(':', '.').Replace('$', '.'));

            // make the dir exist
            string snaffleFilePath = Path.Combine(snafflePath, cleanedPath);
            
            string snaffleDirPath = Path.GetDirectoryName(snaffleFilePath);
            Directory.CreateDirectory(snaffleDirPath);
            File.Copy(sourcePath, (Path.Combine(snafflePath, cleanedPath)), true);
        }
    }
}