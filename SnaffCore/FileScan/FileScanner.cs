﻿using Classifiers;
using SnaffCore.Concurrency;
using System;
using System.IO;
using static SnaffCore.Config.Options;

namespace SnaffCore.FileScan
{
    public class FileScanner
    {
        private BlockingMq Mq { get; set; }

        public FileScanner()
        {
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