using Classifiers;
using SnaffCore.Concurrency;
using System;
using System.IO;
using static SnaffCore.Config.Options;

namespace SnaffCore.FileScan
{
    public class FileScanner
    {
        private BlockingMq Mq { get; set; }
        private int InterestLevel { get; set; }

        public FileScanner(int level)
        {
            InterestLevel = level;
            Mq = BlockingMq.GetMq();
        }
        public void ScanFile(string file)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(file);
                // send the file to all the classifiers.
                foreach (ClassifierRule classifier in MyOptions.FileClassifiers)
                {
                    // Don't send file to classifier if interest level is not high enough
                    if ((classifier.Triage == Triage.Red && InterestLevel > 2) ||
                        (classifier.Triage == Triage.Yellow && InterestLevel > 1) ||
                        (classifier.Triage == Triage.Green && InterestLevel > 0)
                        )
                    {
                        continue;
                    }
                    FileClassifier fileClassifier = new FileClassifier(classifier);
                    if (fileClassifier.ClassifyFile(fileInfo))
                    {
                        return;
                    };
                }
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
                Mq.Trace(file + " path was too long for me to look at.");
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