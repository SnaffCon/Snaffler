using System;
using System.Collections.Generic;
using System.IO;

namespace SnaffCore.ShareScan
{
    public class TreeWalker
    {
        private Config.Config Config { get; set; }
        private FileScanner FileScanner { get; set; }

        public TreeWalker(Config.Config config, string shareRoot)
        {
            Config = config;
            if (shareRoot == null)
            {
                config.Mq.Trace("A null made it into TreeWalker. Wtf.");
                return;
            }

            config.Mq.Trace("About to start a TreeWalker on share " + shareRoot);
            FileScanner = new FileScanner();
            WalkTree(shareRoot);
            config.Mq.Trace("Finished TreeWalking share " + shareRoot);
        }

        public void WalkTree(string shareRoot)
        {
            // Walks a tree checking files and generating results as it goes.
            var dirs = new Stack<string>(20);

            if (!Directory.Exists(shareRoot))
            {
                return;
            }

            dirs.Push(shareRoot);

            while (dirs.Count > 0)
            {
                var currentDir = dirs.Pop();
                string[] subDirs;
                try
                {
                    subDirs = Directory.GetDirectories(currentDir);
                }
                catch (UnauthorizedAccessException e)
                {
                    Config.Mq.Trace(e.ToString());
                    continue;
                }
                catch (DirectoryNotFoundException e)
                {
                    Config.Mq.Trace(e.Message);
                    continue;
                }

                string[] files = null;
                try
                {
                    files = Directory.GetFiles(currentDir);
                }
                catch (UnauthorizedAccessException e)
                {
                    Config.Mq.Trace(e.Message);
                    continue;
                }

                catch (DirectoryNotFoundException e)
                {
                    Config.Mq.Trace(e.Message);
                    continue;
                }

                // check if we actually like the files
                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);

                        var fileResult = FileScanner.Scan(fileInfo, Config);

                        if (fileResult != null)
                        {
                            if (fileResult.WhyMatched != FileScanner.MatchReason.NoMatch)
                            {
                                Config.Mq.FileResult(fileResult);
                            }
                        }
                    }
                    catch (FileNotFoundException e)
                    {
                        // If file was deleted by a separate application
                        //  or thread since the call to TraverseTree()
                        // then just continue.
                        Config.Mq.Trace(e.Message);
                    }
                }

                // Push the subdirectories onto the stack for traversal.
                // This could also be done before handing the files.
                foreach (var dirStr in subDirs)
                {
                    //but skip any dirs in the skiplist.
                    if (!FileScanner.PartialMatchInArray(dirStr, Config.DirSkipList))
                    {
                        dirs.Push(dirStr);
                    }
                }
            }
        }
    }
}