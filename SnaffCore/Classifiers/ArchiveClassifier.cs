using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using SharpCompress.Archives;
using SnaffCore.Concurrency;
using SnaffCore.FileScan;

namespace Classifiers
{
    public class ArchiveClassifier
    {
        // TODO VERY WORK IN PROGRESS

        private BlockingMq Mq { get; set; }

        public ArchiveClassifier(FileInfo fileInfo)
        {
            Mq = BlockingMq.GetMq();
            ClassifyArchive(fileInfo);
        }

        private void ClassifyArchive(FileInfo fileInfo)
        {
            // look inside archives for files we like.
            try
            {
                var archive = ArchiveFactory.Open(fileInfo.FullName);
                foreach (var entry in archive.Entries)
                {
                    if (!entry.IsDirectory)
                    {
                        try
                        {
                            FileScanner fileScanner = new FileScanner(entry.Key);
                        }
                        catch (Exception e)
                        {
                            Mq.Trace(e.ToString());
                        }
                    }
                }
            }
            catch (CryptographicException e)
            {
                Mq.FileResult(new FileResult(fileInfo)
                {
                    MatchedRule = new ClassifierRule() {Triage = Triage.Black, RuleName = "EncryptedArchive"}
                });
            }
            catch (Exception e)
            {
                Mq.Trace(e.ToString());
            }
        }
    }
}