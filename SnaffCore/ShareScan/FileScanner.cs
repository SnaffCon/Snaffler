using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text.RegularExpressions;
using Classifiers;
using SnaffCore.Concurrency;
using Config = SnaffCore.Config.Config;


namespace SnaffCore.ShareScan
{
    public class FileScanner
    {
        // checks a file to see if it's cool or not.
        public void Scan(FileInfo fileInfo)
        {
            BlockingMq Mq = BlockingMq.GetMq();
            Config.Config myConfig = Config.Config.GetConfig();
            // if each check is enabled in FileScannerConfig, run it on the thing.
            foreach (Classifier classifier in myConfig.Options.FileClassifiers)
            {
                classifier.ClassifyFile(fileInfo);
            }
        }

    }
}