using System;
using System.IO;
using SnaffCore.Concurrency;
using static SnaffCore.Config.Options;
using Microsoft.CST.RecursiveExtractor;

namespace SnaffCore.FileScan
{
    public class ArchiveFileScanner
    {
        private BlockingMq Mq { get; set; }

        public ArchiveFileScanner()
        {
            Mq = BlockingMq.GetMq();
        }
        public void ScanArchiveFile(FileEntry fileEntry)
        {
            try
            {
                Console.WriteLine(fileEntry.FullPath);
                /*
                // WHOOPS DO STUFF HERE
                FileInfo fileInfo = new FileInfo(file);
                // send the file to all the classifiers.
                foreach (ClassifierRule classifier in MyOptions.FileClassifiers)
                {
                    FileClassifier fileClassifier = new FileClassifier(classifier);
                    if (fileClassifier.ClassifyFile(fileInfo))
                    {
                        // it returns true if we want to bail out, typically because we hit a discard rule. otherwise it keeps going.
                        return;
                    };
                }
                */
            }
            catch (FileNotFoundException e)
            {
                // If file was deleted by a separate application
                //  or thread since the call to TraverseTree()
                // then just continue.
                Mq.Trace(e.ToString());
                return;
            }
            catch (UnauthorizedAccessException e)
            {
                Mq.Trace(e.ToString());
                return;
            }
            catch (PathTooLongException)
            {
                Mq.Trace(fileEntry.FullPath + " path was too long for me to look at.");
                return;
            }
            catch (Exception e)
            {
                Mq.Trace(e.ToString());
                return;
            }
        }
    }
}
