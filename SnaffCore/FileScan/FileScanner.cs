using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Classifiers;
using SnaffCore.Concurrency;


namespace SnaffCore.FileScan
{
    public class FileScanner
    {
        private Config.Config myConfig { get; set; }
        private BlockingMq Mq { get; set; }

        public FileScanner(string file)
        {
            myConfig = Config.Config.GetConfig();
            Mq = BlockingMq.GetMq();
            ScanFile(file);
        }
        public void ScanFile(string file)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(file);
                // send the file to all the classifiers.
                foreach (Classifier classifier in myConfig.Options.FileClassifiers)
                {
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