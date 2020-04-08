using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Classifiers;
using SnaffCore.Concurrency;
using Config = SnaffCore.Config.Config;


namespace SnaffCore.ShareScan
{
    public class FileScanner
    {
        public void ScanFile(string file)
        {
            BlockingMq Mq = BlockingMq.GetMq();
            Config.Config myConfig = Config.Config.GetConfig();
            try
            {
                FileInfo fileInfo = new FileInfo(file);
                // send the file to all the classifiers.
                foreach (Classifier classifier in myConfig.Options.FileClassifiers)
                {
                    classifier.ClassifyFile(fileInfo);
                }
            }
            catch (FileNotFoundException e)
            {
                // If file was deleted by a separate application
                //  or thread since the call to TraverseTree()
                // then just continue.
                Mq.Trace(e.Message);
                return;
            }
            catch (UnauthorizedAccessException e)
            {
                Mq.Trace(e.Message);
                return;
            }
            catch (PathTooLongException)
            {
                Mq.Trace(file + " path was too long for me to look at.");
                return;
            }
            catch (Exception e)
            {
                Mq.Trace(e.Message);
                return;
            }
        }
    }
}