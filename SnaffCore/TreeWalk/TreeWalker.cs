using SnaffCore.Classifiers;
using SnaffCore.Checkpoint;
using SnaffCore.Concurrency;
using SnaffCore.Config;
using SnaffCore.FileScan;
using System;
using System.IO;
using System.Text.RegularExpressions;
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

            // If resuming from a checkpoint, skip directories we already processed.
            var checkpointMgr = CheckpointManager.GetInstance();
            if (checkpointMgr != null && checkpointMgr.IsDirectoryScanned(currentDir))
            {
                Mq.Trace("[Checkpoint] Skipping already-scanned directory: " + currentDir);
                return;
            }

            // Record this directory so it is skipped on any future resume.
            checkpointMgr?.MarkDirectoryScanned(currentDir);

            // SCCM ContentLib($)
            try
            {
                var currentDirInfo = new DirectoryInfo(currentDir);
                string currentDirectoryName = currentDirInfo.Name; // Remove paths, keep dirname only
                if (currentDirectoryName == @"SCCMContentLib" || currentDirectoryName == @"SCCMContentLib$")
                {
                    Mq.Info("SCCM content library found: " + currentDir);
                    string dataLibDir = Path.Combine(currentDir, "DataLib"); // As full path
                    if (!Directory.Exists(dataLibDir))
                    {
                        Mq.Error("SCCM content library found but no DataLib found: " + dataLibDir);
                        return;
                    }
                    Mq.Info("SCCM content library: Entering into datalib: " + dataLibDir);
                    WalkSccmTree(dataLibDir, currentDir); // With base path name
                    return;
                }
            }
            catch (Exception e)
            {
                Mq.Degub(e.ToString());
                //continue;
            }


            // Existing code:
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

            try
            {
                string[] subDirs = Directory.GetDirectories(currentDir);
                // Create a new treewalker task for each subdir.
                if (subDirs.Length >= 1)
                {

                    foreach (string dirStr in subDirs)
                    {
                        bool scanDir = true;
                        foreach (ClassifierRule classifier in MyOptions.DirClassifiers)
                        {
                            try
                            {
                                DirClassifier dirClassifier = new DirClassifier(classifier);
                                DirResult dirResult = dirClassifier.ClassifyDir(dirStr);

                                if (dirResult.ScanDir == false)
                                {
                                    scanDir = false;
                                    break;
                                }
                            }
                            catch (Exception e)
                            {
                                Mq.Trace(e.ToString());
                                continue;
                            }
                        }
                        if (scanDir == true)
                        {
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
                        else
                        {
                            Mq.Trace("Skipped scanning on " + dirStr + " due to Discard rule match.");
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
            catch (IOException)
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
        public void WalkSccmTree(string currentDir, string sccmBaseDir)
        {
            // Walks a tree checking files and generating results as it goes.
            if (!Directory.Exists(currentDir))
            {
                return;
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
                            //FileScanner.ScanFile(file);

                            FileInfo fileInfo = new FileInfo(file);

                            // Check if INI
                            if (fileInfo.Extension != ".INI")
                            {
                                Mq.Trace("Skipping file in DataLib but does not extention .INI; Something wrong: " + fileInfo.FullName);
                                return;
                            }

                            // Check if it points to real file
                            string fileString = File.ReadAllText(fileInfo.FullName);
                            if(!(fileString.StartsWith(@"[File]"))){
                                Mq.Trace("Skipping file in DataLib but does not points to any file: " + fileInfo.FullName);
                                return;
                            }

                            // Obtain hash
                            string pattern = @"Hash=([0-9A-Fa-f]+)";
                            string hashValueText;
                            if (Regex.IsMatch(fileString, pattern))
                            {
                                hashValueText = Regex.Match(fileString, pattern).Groups[1].Value;
                            }
                            else
                            {
                                Mq.Trace("Skipping file in DataLib but does not have hash of any file: " + fileInfo.FullName);
                                return;
                            }

                            string targetDirName = hashValueText.Substring(0, 4);

                            // strip off .INI to get actual name
                            //string targetFileName = fileInfo.FullName.Replace(".INI", "");
                            string tmpFullFileName = fileInfo.FullName;
                            string alternativeFullFileName = tmpFullFileName.Substring(0, (tmpFullFileName.Length - 4)); // Remove ".INI";
                            AlternativeFileInfo altFileInfo = new AlternativeFileInfo(alternativeFullFileName);

                            // Calculate real path
                            string targetFilePathName = Path.Combine(sccmBaseDir, @"FileLib", targetDirName, hashValueText);
                            FileInfo contentFileInfo = new FileInfo(targetFilePathName);

                            Mq.Trace("We can look at file [" + contentFileInfo.FullName + " ] reffered by [ " + fileInfo.FullName + " ] should be handled as [ " + alternativeFullFileName + " ]");

                            /*
                            // for Debug
                            if (!(contentFileInfo.Exists))
                            {
                                Mq.Error("File not exist: " + contentFileInfo.FullName);
                            }
                            */

                            FileTaskScheduler.New(() =>
                            {
                                try
                                {
                                    FileScanner.ScanFile(targetFilePathName, altFileInfo);
                                }
                                catch (Exception e)
                                {
                                    Mq.Error("Exception in FileScanner task for file " + file);
                                    Mq.Trace(e.ToString());
                                }
                            });


                            return;



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

            try
            {
                string[] subDirs = Directory.GetDirectories(currentDir);
                // Create a new treewalker task for each subdir.
                if (subDirs.Length >= 1)
                {

                    foreach (string dirStr in subDirs)
                    {
                        //foreach (ClassifierRule classifier in MyOptions.DirClassifiers) // No rules should be applied for SCCMContentLib($)

                        bool scanDir = true;
                        foreach (ClassifierRule classifier in MyOptions.DirClassifiers)
                        {
                            try
                            {
                                DirClassifier dirClassifier = new DirClassifier(classifier);
                                DirResult dirResult = dirClassifier.ClassifyDir(dirStr);

                                if (dirResult.ScanDir == false)
                                {
                                    scanDir = false;
                                    break;
                                }
                            }
                            catch (Exception e)
                            {
                                Mq.Trace(e.ToString());
                                continue;
                            }
                        }
                        if (scanDir == true)
                        {
                            TreeTaskScheduler.New(() =>
                            {
                                try
                                {
                                    WalkSccmTree(dirStr, sccmBaseDir);
                                }
                                catch (Exception e)
                                {
                                    Mq.Error("Exception in TreeWalker task for dir " + dirStr);
                                    Mq.Error(e.ToString());
                                }
                            });
                        }
                        else
                        {
                            Mq.Trace("Skipped scanning on " + dirStr + " due to Discard rule match.");
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
            catch (IOException)
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