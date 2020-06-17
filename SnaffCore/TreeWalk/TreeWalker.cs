using Classifiers;
using SnaffCore.Concurrency;
using SnaffCore.FileScan;
using System;
using System.IO;
using static SnaffCore.Config.Options;

namespace SnaffCore.TreeWalk
{
    public class TreeWalker
    {
        private BlockingMq Mq { get; set; }
        private BlockingStaticTaskScheduler FileTaskScheduler { get; set; }
        private BlockingStaticTaskScheduler TreeTaskScheduler { get; set; }
        private FileScanner FileScanner { get; set; }

        public TreeWalker()
        {
            Mq = BlockingMq.GetMq();

            FileTaskScheduler = SnaffCon.GetFileTaskScheduler();
            TreeTaskScheduler = SnaffCon.GetTreeTaskScheduler();
            FileScanner = SnaffCon.GetFileScanner();
        }

        public void WalkTree(string currentDir)
        {
             // Walks a tree checking files and generating results as it goes.
             
             if (!Directory.Exists(currentDir))
             {
                 return;
             }
             
             try
             {
                 string[] subDirs = Directory.GetDirectories(currentDir);
                 // Create a new treewalker task for each subdir.
                 if (subDirs.Length >= 1)
                 {
             
                     foreach (string dirStr in subDirs)
                     {
                         foreach (ClassifierRule classifier in MyOptions.DirClassifiers)
                         {
                             try
                             {
                                 DirClassifier dirClassifier = new DirClassifier(classifier);
                                 DirResult dirResult = dirClassifier.ClassifyDir(dirStr);
             
                                 if (dirResult.ScanDir)
                                 {
                                     //dirs.Push(dirStr);
                                     TreeTaskScheduler.New(() =>
                                     {
                                         try
                                         {
                                             WalkTree(dirStr);
                                         }
                                         catch (Exception e)
                                         {
                                             Mq.Error("Exception in TreeWalker task for dir " + dirStr);
                                             Mq.Error(e.ToString());
                                         }
                                     });
                                 }
                             }
                             catch (Exception e)
                             {
                                 Mq.Trace(e.ToString());
                                 continue;
                             }
                         }
                     }
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
             catch (Exception e)
             {
                 Mq.Trace(e.ToString());
                 //continue;
             }
             
             try
             {
                 string[] files = Directory.GetFiles(currentDir);
                 // check if we actually like the files
                 foreach (string file in files)
                 {
                     FileTaskScheduler.New(() =>
                     {
                         try
                         {
                             FileScanner.ScanFile(file);
                         }
                         catch (Exception e)
                         {
                             Mq.Error("Exception in FileScanner task for file " + file);
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
             catch (Exception e)
             {
                 Mq.Trace(e.ToString());
                 //continue;
             }

        }
    }
}