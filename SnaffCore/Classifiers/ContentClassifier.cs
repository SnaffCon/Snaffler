using SnaffCore.Concurrency;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using static SnaffCore.Config.Options;

#if ULTRASNAFFLER
using Toxy;
#endif

namespace SnaffCore.Classifiers
{
    public class ContentClassifier
    {
        private ClassifierRule ClassifierRule { get; set; }

        public ContentClassifier(ClassifierRule inRule)
        {
            this.ClassifierRule = inRule;
        }

        public void ClassifyContent(FileInfo fileInfo)
        {
            BlockingMq Mq = BlockingMq.GetMq();
            FileResult fileResult;
            try
            {
                if (MyOptions.MaxSizeToGrep >= fileInfo.Length)
                {
                    // figure out if we need to look at the content as bytes or as string.
                    switch (ClassifierRule.MatchLocation)
                    {
                        case MatchLoc.FileContentAsBytes:
                            byte[] fileBytes = File.ReadAllBytes(fileInfo.FullName);
                            if (ByteMatch(fileBytes))
                            {
                                fileResult = new FileResult(fileInfo)
                                {
                                    MatchedRule = ClassifierRule
                                };
                                // if the file was list-only, don't bother sending a result back to the user.
                                if (!fileResult.RwStatus.CanRead && !fileResult.RwStatus.CanModify && !fileResult.RwStatus.CanWrite) { return; };
                                Mq.FileResult(fileResult);
                            }
                            return;
                        case MatchLoc.FileContentAsString:
                            try
                            {
                                string fileString;

#if ULTRASNAFFLER
                                // if it's an office doc or a PDF or something, parse it to a string first i guess?
                                List<string> parsedExtensions = new List<string>()
                                {
                                    ".doc",".docx",".xls",".xlsx",".eml",".msg",".pdf",".ppt",".pptx",".rtf",".docm",".xlsm",".pptm",".dot",".dotx",".dotm",".xlt",".xlsm",".xltm"
                                };

                                if (parsedExtensions.Contains(fileInfo.Extension))
                                {
                                    fileString = ParseFileToString(fileInfo);
                                }
                                else
                                {
                                    fileString = File.ReadAllText(fileInfo.FullName);
                                }
#else
                                fileString = File.ReadAllText(fileInfo.FullName);
#endif
                                TextClassifier textClassifier = new TextClassifier(ClassifierRule);
                                TextResult textResult = textClassifier.TextMatch(fileString);
                                if (textResult != null)
                                {
                                    fileResult = new FileResult(fileInfo)
                                    {
                                        MatchedRule = ClassifierRule,
                                        TextResult = textResult
                                    };
                                    // if the file was list-only, don't bother sending a result back to the user.
                                    if (!fileResult.RwStatus.CanRead && !fileResult.RwStatus.CanModify && !fileResult.RwStatus.CanWrite) { return; };
                                    Mq.FileResult(fileResult);
                                }
                            }
                            catch (UnauthorizedAccessException)
                            {
                                return;
                            }
                            catch (IOException)
                            {
                                return;
                            }
                            return;
                        case MatchLoc.FileLength:
                            bool lengthResult = SizeMatch(fileInfo);
                            if (lengthResult)
                            {
                                fileResult = new FileResult(fileInfo)
                                {
                                    MatchedRule = ClassifierRule
                                };

                                // if the file was list-only, don't bother sending a result back to the user.
                                if (!fileResult.RwStatus.CanRead && !fileResult.RwStatus.CanModify && !fileResult.RwStatus.CanWrite) { return; };
                                Mq.FileResult(fileResult);
                            }
                            return;
                        case MatchLoc.FileMD5:
                            bool Md5Result = MD5Match(fileInfo);
                            if (Md5Result)
                            {
                                fileResult = new FileResult(fileInfo)
                                {
                                    MatchedRule = ClassifierRule
                                };

                                // if the file was list-only, don't bother sending a result back to the user.
                                if (!fileResult.RwStatus.CanRead && !fileResult.RwStatus.CanModify && !fileResult.RwStatus.CanWrite) { return; };
                                Mq.FileResult(fileResult);
                            }
                            return;
                        default:
                            Mq.Error("You've got a misconfigured file ClassifierRule named " + ClassifierRule.RuleName + ".");
                            return;
                    }
                }
                else
                {
                    Mq.Trace("The following file was bigger than the MaxSizeToGrep config parameter:" + fileInfo.FullName);
                }
            }
            catch (UnauthorizedAccessException)
            {
                Mq.Error($"Not authorized to access file: {fileInfo.FullName}");
                return;
            }
            catch (IOException e)
            {
                Mq.Error($"IO Exception on file: {fileInfo.FullName}. {e.Message}");
                return;
            }
            catch (Exception e)
            {
                Mq.Error(e.ToString());
                return;
            }
        }

        public bool SizeMatch(FileInfo fileInfo)
        {
            if (this.ClassifierRule.MatchLength == fileInfo.Length)
            {
                return true;
            }
            return false;
        }

        public bool MD5Match(FileInfo fileInfo)
        {
            string md5Sum = GetMD5HashFromFile(fileInfo.FullName);
            if (md5Sum == this.ClassifierRule.MatchMD5.ToUpper())
            {
                return true;
            }
            return false;
        }

        protected string GetMD5HashFromFile(string fileName)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(fileName))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }
        }

#if ULTRASNAFFLER
        public string ParseFileToString(FileInfo fileInfo)
        {
            ParserContext context = new ParserContext(fileInfo.FullName);
            ITextParser parser = ParserFactory.CreateText(context);

                string doc = parser.Parse();
            return doc;
        }
#endif

        public bool ByteMatch(byte[] fileBytes)
        {
            // TODO
            throw new NotImplementedException(message: "Haven't implemented byte-based content searching yet lol.");
        }
    }
}