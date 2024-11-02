using System;
using System.IO;
using SnaffCore.Concurrency;
using SnaffCore.FileScan;
using static SnaffCore.Config.Options;
using Microsoft.CST.RecursiveExtractor;

namespace SnaffCore.TreeWalk
{
    public class ArchiveWalker
    {

        private BlockingMq Mq { get; set; }
        private BlockingStaticTaskScheduler ArchiveFileTaskScheduler { get; set; }
        private BlockingStaticTaskScheduler ArchiveWalkerTaskScheduler { get; set; }
        private ArchiveFileScanner ArchiveFileScanner { get; set; }

        public ArchiveWalker()
        {
            Mq = BlockingMq.GetMq();

            ArchiveFileTaskScheduler = SnaffCon.GetArchiveFileTaskScheduler();
            ArchiveWalkerTaskScheduler = SnaffCon.GetArchiveWalkerTaskScheduler();
            ArchiveFileScanner = SnaffCon.GetArchiveFileScanner();
        }

        public void WalkArchive(string archiveFilePath)
        {
            // Enumerates the files in an archive and passes them off for scanning.

            if (!File.Exists(archiveFilePath))
            {
                return;
            }
            try
            {
                var extractor = new Extractor();
                var files = extractor.Extract(archiveFilePath);
                foreach (var file in files)
                {
                    Console.WriteLine(file.FullPath);
          
                        ArchiveFileTaskScheduler.New(() =>
                        {
                            try
                            {
                                ArchiveFileScanner.ScanArchiveFile(file);
                            }
                            catch (Exception e)
                            {
                                Mq.Error("Exception in ArchiveFileScanner task for file " + file);
                                Mq.Trace(e.ToString());
                            }
                        });
                    }
                }
            catch (UnauthorizedAccessException)
            {
                //Mq.Trace(e.ToString());
                //continue;
            }
            catch (DirectoryNotFoundException)
            {
                //Mq.Trace(e.ToString());
                //continue;
            }
            catch (IOException)
            {
                //Mq.Trace(e.ToString());
                //continue;
            }
            catch (Exception e)
            {
                Mq.Degub(e.ToString());
                //continue;
            }

        }
    }
}
