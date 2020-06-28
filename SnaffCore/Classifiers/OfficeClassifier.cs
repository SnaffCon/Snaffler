using SharpCompress.Archives;
using SnaffCore.Concurrency;
using SnaffCore.FileScan;
using System;
using System.IO;
using System.Security.Cryptography;
using Classifiers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpCompress.Readers;

namespace SnaffCore.Classifiers
{

    public class OfficeClassifier
    {
        // TODO VERY WORK IN PROGRESS

        private BlockingMq Mq { get; set; }
        private FileScanner FileScanner { get; set; }

        public OfficeClassifier()
        {
            Mq = BlockingMq.GetMq();
        }

        public void ClassifyNSOfficeWithMacro(FileInfo fileInfo)
        {
            try
            {
                using (Stream stream = File.OpenRead(fileInfo.FullName))
                using (var reader = ReaderFactory.Open(stream))
                {
                    while (reader.MoveToNextEntry())
                    {
                        if (reader.Entry.Key == "[Content_Types].xml")
                        {
                            try
                            {
                                MemoryStream stream2 = new MemoryStream();
                                using (Stream entryStream = reader.OpenEntryStream())
                                {
                                    entryStream.CopyTo(stream2);
                                }

                                byte[] bytes = stream2.ToArray();

                                string entryContents = Encoding.ASCII.GetString(bytes);

                                if (entryContents.Contains("vnd.ms-office.vbaProject"))
                                {
                                    Mq.FileResult(new FileResult(fileInfo)
                                    {
                                        MatchedRule = new ClassifierRule()
                                            {Triage = Triage.Black, RuleName = "NSOfficeWithMacro"}
                                    });
                                }

                                else
                                {
                                    Mq.FileResult(new FileResult(fileInfo)
                                    {
                                        MatchedRule = new ClassifierRule()
                                            {Triage = Triage.Green, RuleName = "NSOfficeNoMacro"}
                                    });
                                }

                                /*
                                IArchive archive = ArchiveFactory.Open(fileInfo.FullName);
                                foreach (IArchiveEntry entry in archive.Entries)
                                {
                                    if (entry.Key == "[Content_Types].xml")
                                    {
                                        try
                                        {

                                            MemoryStream stream = new MemoryStream();
                                            */


                            }
                            catch (Exception e)
                            {
                                Mq.Trace(e.ToString());
                            }
                        }
                    }
                }
            }
            catch (CryptographicException)
            {
                Mq.FileResult(new FileResult(fileInfo)
                {
                    MatchedRule = new ClassifierRule() {Triage = Triage.Black, RuleName = "EncryptedArchive"}
                });
            }
            catch (System.InvalidOperationException e)
            {
                Mq.Error("<<" + fileInfo.FullName + ">> " + "looks like it might be encrypted.");
            }
            catch (System.IO.IOException e)
            {
                Mq.Error("<<" + fileInfo.FullName + ">> " + e.ToString());
            }
            catch (Exception e)
            {
                Mq.Error(e.ToString());
            }
        }

        public void ClassifyOGOfficeWithMacro(FileInfo fileInfo)
        {
            try
            {
                string magic1 = "[Host Extender Info]";
                byte[] magicbytes1 = Encoding.ASCII.GetBytes(magic1);
                string magic2 = "[Workspace]";
                byte[] magicbytes2 = Encoding.ASCII.GetBytes(magic2);

                Byte[] bytes = File.ReadAllBytes(fileInfo.FullName);

                int index1 = SearchBytes(bytes, magicbytes1);
                int index2 = SearchBytes(bytes, magicbytes2);
                if ((index1 != -1) && (index2 != -1))
                {
                    Mq.FileResult(new FileResult(fileInfo)
                    {
                        MatchedRule = new ClassifierRule() { Triage = Triage.Black, RuleName = "OGOfficeWithMacro" }
                    });
                }
                
                else
                {
                    Mq.FileResult(new FileResult(fileInfo)
                    {
                        MatchedRule = new ClassifierRule() { Triage = Triage.Green, RuleName = "OGOfficeNoMacro" }
                    });
                }
                
            }
            catch (CryptographicException)
            {
                Mq.FileResult(new FileResult(fileInfo)
                {
                    MatchedRule = new ClassifierRule() { Triage = Triage.Black, RuleName = "EncryptedArchive" }
                });
            }
            catch (System.InvalidOperationException e)
            {
                Mq.Error("<<" + fileInfo.FullName + ">> " + "looks like it might be encrypted.");
            }
            catch (System.IO.IOException e)
            {
                Mq.Error("<<" + fileInfo.FullName + ">> " + e.ToString());
            }
            catch (Exception e)
            {
                Mq.Error(e.ToString());
            }
        }

        static int SearchBytes(byte[] haystack, byte[] needle)
        {
            var len = needle.Length;
            var limit = haystack.Length - len;
            for (var i = 0; i <= limit; i++)
            {
                var k = 0;
                for (; k < len; k++)
                {
                    if (needle[k] != haystack[i + k]) break;
                }
                if (k == len) return i;
            }
            return -1;
        }
    }
}
