using SharpCompress.Archives;
using SnaffCore.Concurrency;
using SnaffCore.FileScan;
using System;
using System.IO;
using System.Security.Cryptography;
using SnaffCore;

namespace Classifiers
{
    public class ArchiveClassifier
    {
        // TODO VERY WORK IN PROGRESS

        private BlockingMq Mq { get; set; }
        private FileScanner FileScanner {get; set;}

        public ArchiveClassifier(FileInfo fileInfo)
        {
            Mq = BlockingMq.GetMq();
            FileScanner = SnaffCon.GetFileScanner();
        }
        private void ClassifyArchive(FileInfo fileInfo)
        {
            // look inside archives for files we like.
            try
            {
                IArchive archive = ArchiveFactory.Open(fileInfo.FullName);
                foreach (IArchiveEntry entry in archive.Entries)
                {
                    if (!entry.IsDirectory)
                    {
                        try
                        {
                            FileScanner.ScanFile(entry.Key);
                        }
                        catch (Exception e)
                        {
                            Mq.Trace(e.ToString());
                        }
                    }
                }
            }
            catch (CryptographicException)
            {
                Mq.FileResult(new FileResult(fileInfo)
                {
                    MatchedRule = new ClassifierRule() { Triage = Triage.Black, RuleName = "EncryptedArchive" }
                });
            }
            catch (Exception e)
            {
                Mq.Trace(e.ToString());
            }
        }
    }
}