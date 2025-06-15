using SnaffCore;
using SnaffCore.Concurrency;
using SnaffCore.Config;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading;

namespace Snaffler
{
    public class SnaffleRunner
    {
        private BlockingMq Mq { get; set; }
        private LogLevel LogLevel { get; set; }
        private Options Options { get; set; }

        private string _hostString;

        private string fileResultTemplate { get; set; }
        private string shareResultTemplate { get; set; }
        private string dirResultTemplate { get; set; }

        private string hostString()
        {
            if (string.IsNullOrWhiteSpace(_hostString))
            {
                _hostString = "[" + System.Security.Principal.WindowsIdentity.GetCurrent().Name + "@" + System.Net.Dns.GetHostName() + "]";
            }

            return _hostString;
        }

        public async Task RunAsync(string[] args)
        {
            // prime the hoststring lazy instantiator
            hostString();
            // print the thing
            PrintBanner();
            // set up the message queue for operation
            BlockingMq.MakeMq();
            // get a handle to the message queue singleton
            Mq = BlockingMq.GetMq();
            // prime the UI handler
            SnaffCon controller = null;
            try
            {
                // parse cli opts in
                Options = await Config.ParseAsync(args);

                if (Options == null)
                {
                    // bail out because the user was just running help
                    return;
                }

                // set up the  TSV output if the flag is set
                if (Options.LogTSV)
                {
                    fileResultTemplate = "{0}" + Options.Separator + "{1}" + Options.Separator + "{2}" + Options.Separator + "{3}" + Options.Separator + "{4}" + Options.Separator + "{5}" + Options.Separator + "{6}" + Options.Separator + "{7:u}" + Options.Separator + "{8}" + Options.Separator + "{9}";
                    shareResultTemplate = "{0}" + Options.Separator + "{1}" + Options.Separator + "{2}";
                    dirResultTemplate = "{0}" + Options.Separator + "{1}";
                }
                // otherwise just do the normal thing
                else
                {
                    // treat all as strings except LastWriteTime {6}
                    fileResultTemplate = "{{{0}}}<{1}|{2}{3}{4}|{5}|{6}|{7:u}>({8}) {9}";
                    shareResultTemplate = "{{{0}}}<{1}>({2}) {3}";
                    dirResultTemplate = "{{{0}}}({1})";
                }
                //------------------------------------------
                // set up logging
                //------------------------------------------

                ParseLogLevelString(Options.LogLevelString);

                Logger.Configure(
                    Options.LogToConsole,
                    Options.LogToFile,
                    Options.LogFilePath,
                    LogLevel,
                    Options.LogType,
                    Options.Separator);

                //-------------------------------------------

                if (Options.Snaffle && (Options.SnafflePath.Length > 4))
                {
                    Directory.CreateDirectory(Options.SnafflePath);
                }

                controller = new SnaffCon(Options);

                Task controllerTask = Task.Run(() => controller.ExecuteAsync());
                await HandleOutputAsync();
                await controllerTask;
                Logger.Close();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                DumpQueue();
            }
        }

        private void DumpQueue()
        {
#if NETFRAMEWORK
            BlockingMq Mq = BlockingMq.GetMq();
            while (Mq.Collection.TryTake(out SnafflerMessage message))
#else
            BlockingMq Mq = BlockingMq.GetMq();
            while (Mq.Reader.TryRead(out SnafflerMessage message))
#endif
            {
                // emergency dump of queue contents to console
                Console.WriteLine(message.Message);
            }
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.ReadKey();
            }
        }

        private async Task<bool> HandleOutputAsync()
        {
            BlockingMq Mq = BlockingMq.GetMq();
#if NETFRAMEWORK
            foreach (SnafflerMessage message in Mq.Collection.GetConsumingEnumerable())
#else
            await foreach (SnafflerMessage message in Mq.Reader.ReadAllAsync())
#endif
            {
                if (Options.LogType == LogType.Plain)
                {
                    ProcessMessage(message);
                }
                else if (Options.LogType == LogType.JSON)
                {
                    await ProcessMessageJSONAsync(message);
                }

                // catch terminating messages and bail out
                if ((message.Type == SnafflerMessageType.Fatal) || (message.Type == SnafflerMessageType.Finish))
                {
                    return true;
                }
            }
            return false;
        }

        private void ProcessMessage(SnafflerMessage message)
        {
            string prefix = hostString() + Options.Separator +
                message.DateTime.ToUniversalTime().ToString("u") + Options.Separator;

            switch (message.Type)
            {
                case SnafflerMessageType.Trace:
                    Logger.Trace(message.DateTime, prefix, message.Message);
                    break;
                case SnafflerMessageType.Degub:
                    Logger.Debug(message.DateTime, prefix, message.Message);
                    break;
                case SnafflerMessageType.Info:
                    Logger.Info(message.DateTime, prefix, message.Message);
                    break;
                case SnafflerMessageType.FileResult:
                    Logger.File(message.DateTime, prefix, FileResultLogFromMessage(message));
                    break;
                case SnafflerMessageType.DirResult:
                    Logger.Dir(message.DateTime, prefix, DirResultLogFromMessage(message));
                    break;
                case SnafflerMessageType.ShareResult:
                    Logger.Share(message.DateTime, prefix, ShareResultLogFromMessage(message));
                    break;
                case SnafflerMessageType.Error:
                    Logger.Error(message.DateTime, prefix, message.Message);
                    break;
                case SnafflerMessageType.Fatal:
                    Logger.Fatal(message.DateTime, prefix, message.Message);
                    if (Debugger.IsAttached)
                    {
                        Console.ReadKey();
                    }
                    break;
                case SnafflerMessageType.Finish:
                    Logger.Info(message.DateTime, prefix, "Snaffler out.");
                    
                    if (Debugger.IsAttached)
                    {
                        Console.WriteLine("Press any key to exit.");
                        Console.ReadKey();
                    }
                    break;
            }
        }

        private async Task ProcessMessageJSONAsync(SnafflerMessage message)
        {
            string prefix = hostString() + Options.Separator;

            switch (message.Type)
            {
                case SnafflerMessageType.Trace:
                    Logger.Trace(message.DateTime, prefix, message.Message, message);
                    break;
                case SnafflerMessageType.Degub:
                    Logger.Debug(message.DateTime, prefix, message.Message, message);
                    break;
                case SnafflerMessageType.Info:
                    Logger.Info(message.DateTime, prefix, message.Message, message);
                    break;
                case SnafflerMessageType.FileResult:
                    Logger.File(message.DateTime, prefix, FileResultLogFromMessage(message), message);
                    break;
                case SnafflerMessageType.DirResult:
                    Logger.Dir(message.DateTime, prefix, DirResultLogFromMessage(message), message);
                    break;
                case SnafflerMessageType.ShareResult:
                    Logger.Share(message.DateTime, prefix, ShareResultLogFromMessage(message), message);
                    break;
                case SnafflerMessageType.Error:
                    Logger.Error(message.DateTime, prefix, message.Message, message);
                    break;
                case SnafflerMessageType.Fatal:
                    Logger.Fatal(message.DateTime, prefix, message.Message, message);
                    if (Debugger.IsAttached)
                    {
                        Console.ReadKey();
                    }
                    break;
                case SnafflerMessageType.Finish:
                    Logger.Info(message.DateTime, prefix, "Snaffler out.");

                    if (Debugger.IsAttached)
                    {
                        Console.WriteLine("Press any key to exit.");
                        Console.ReadKey();
                    }
                    if (Options.LogType == LogType.JSON)
                    {
                        Logger.Info(message.DateTime, prefix, "Normalising output, please wait...");
                        await FixJSONOutputAsync();
                    }
                    break;
            }
        }

        public string ShareResultLogFromMessage(SnafflerMessage message)
        {
            string sharePath = message.ShareResult.SharePath;
            string triage = message.ShareResult.Triage.ToString();
            string shareComment = message.ShareResult.ShareComment;

            string rwString = "";
            if (message.ShareResult.RootReadable)
            {
                rwString = rwString + "R";
            }
            if (message.ShareResult.RootWritable)
            {
                rwString = rwString + "W";
            }
            if (message.ShareResult.RootModifyable)
            {
                rwString = rwString + "M";
            }

            return string.Format(shareResultTemplate, triage, sharePath, rwString, shareComment);
        }

        public string DirResultLogFromMessage(SnafflerMessage message)
        {
            string sharePath = message.DirResult.DirPath;
            string triage = message.DirResult.Triage.ToString();
            return string.Format(dirResultTemplate, triage, sharePath);
        }

        public string FileResultLogFromMessage(SnafflerMessage message)
        {
            try
            {
                string matchedclassifier = message.FileResult.MatchedRule.RuleName;
                string triageString = message.FileResult.MatchedRule.Triage.ToString();
                DateTime modifiedStamp = message.FileResult.FileInfo.LastWriteTime.ToUniversalTime();

                string canread = "";
                if (message.FileResult.RwStatus.CanRead)
                {
                    canread = "R";
                }

                string canwrite = "";
                if (message.FileResult.RwStatus.CanWrite)
                {
                    canwrite = "W";
                }

                string canmodify = "";
                if (message.FileResult.RwStatus.CanModify)
                {
                    canmodify = "M";
                }

                string matchedstring = "";

                long fileSize = message.FileResult.FileInfo.Length;

                string fileSizeString;

                // TSV output will probably be machine-consumed.  Don't pretty it up.
                if (Options.LogTSV)
                {
                    fileSizeString = fileSize.ToString();
                }
                else
                {
                    fileSizeString = BytesToString(fileSize);
                }

                string filepath = message.FileResult.FileInfo.FullName;

                string matchcontext = "";
                if (message.FileResult.TextResult != null)
                {
                    matchedstring = message.FileResult.TextResult.MatchedStrings[0];
                    matchcontext = message.FileResult.TextResult.MatchContext;
                    matchcontext = Regex.Replace(matchcontext, @"\r\n?|\n", "\\n"); // Replace newlines with \n for consistent log lines
                }

                return string.Format(fileResultTemplate, triageString, matchedclassifier, canread, canwrite, canmodify, matchedstring, fileSizeString, modifiedStamp,
                    filepath, matchcontext);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine(message.FileResult.FileInfo.FullName);
                return "";
            }
        }
        private void ParseLogLevelString(string logLevelString)
        {
            switch (logLevelString.ToLower())
            {
                case "debug":
                    LogLevel = LogLevel.Debug;
                    Mq.Degub("Set verbosity level to degub.");
                    break;
                case "degub":
                    LogLevel = LogLevel.Debug;
                    Mq.Degub("Set verbosity level to degub.");
                    break;
                case "trace":
                    LogLevel = LogLevel.Trace;
                    Mq.Degub("Set verbosity level to trace.");
                    break;
                case "data":
                    LogLevel = LogLevel.Warn;
                    Mq.Degub("Set verbosity level to data.");
                    break;
                case "info":
                    LogLevel = LogLevel.Info;
                    Mq.Degub("Set verbosity level to info.");
                    break;
                default:
                    LogLevel = LogLevel.Info;
                    Mq.Error("Invalid verbosity level " + logLevelString +
                             " falling back to default level (info).");
                    break;
            }
        }

        private static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "kB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num) + suf[place];
        }

        public void WriteColor(string textToWrite, ConsoleColor fgColor)
        {
            if (Console.IsOutputRedirected)
            {
                Console.Write(textToWrite);
                return;
            }

            Console.ForegroundColor = fgColor;
            Console.Write(textToWrite);
            Console.ResetColor();
        }

        public void WriteColorLine(string textToWrite, ConsoleColor fgColor)
        {
            if (Console.IsOutputRedirected)
            {
                Console.WriteLine(textToWrite);
                return;
            }

            Console.ForegroundColor = fgColor;
            Console.WriteLine(textToWrite);
            Console.ResetColor();
        }

        public void PrintBanner()
        {
            string[] barfLines = new[]
            {
                @" .::::::.:::.    :::.  :::.    .-:::::'.-:::::':::    .,:::::: :::::::..   ",
                @";;;`    ``;;;;,  `;;;  ;;`;;   ;;;'''' ;;;'''' ;;;    ;;;;'''' ;;;;``;;;;  ",
                @"'[==/[[[[, [[[[[. '[[ ,[[ '[[, [[[,,== [[[,,== [[[     [[cccc   [[[,/[[['  ",
                @"  '''    $ $$$ 'Y$c$$c$$$cc$$$c`$$$'`` `$$$'`` $$'     $$""""   $$$$$$c    ",
                @" 88b    dP 888    Y88 888   888,888     888   o88oo,.__888oo,__ 888b '88bo,",
                @"  'YMmMY'  MMM     YM YMM   ''` 'MM,    'MM,  ''''YUMMM''''YUMMMMMMM   'W' ",
                @"                         by l0ss and Sh3r4 - github.com/SnaffCon/Snaffler  "
            };

            ConsoleColor[] patternOne =
            {
                ConsoleColor.Red,
                ConsoleColor.DarkYellow,
                ConsoleColor.Yellow,
                ConsoleColor.Green,
                ConsoleColor.Blue,
                ConsoleColor.DarkMagenta,
                ConsoleColor.White
            };

            int i = 0;
            foreach (string barfLine in barfLines)
            {
                string barfOne = barfLine;
                WriteColorLine(barfOne, patternOne[i]);
                i += 1;
            }

            Console.WriteLine("\n");
        }

        //This is probably slow but it is a quick and easy fix for now.
        private async Task FixJSONOutputAsync()
        {
            //Rename the log file temporarily
            File.Move(Options.LogFilePath, Options.LogFilePath + ".tmp");
            //Prepare the normalised file
            using StreamWriter file = new StreamWriter(Options.LogFilePath);
            //Read in the original log file to an array
            string[] lines = await FileCompat.ReadAllLinesAsync(Options.LogFilePath + ".tmp");
            //Write the surrounding template that we need.
            await file.WriteAsync("{\"entries\": [\n");
            //Write all the lines into the new file but add a comma after all but the last so it becomes valid JSON.
            for (int ii = 0; ii < lines.Length -1; ii++)
            {
                await file.WriteLineAsync(lines[ii] + ",");
            }
            //Add the last line but without a comma
            await file.WriteLineAsync(lines[lines.Length - 1]);
            //Close out the file's contents with the last of the JSON to make it valid.
            await file.WriteAsync("]\n}");
            //Flush the output
            await file.FlushAsync();
            //Delete the temporary file.
            File.Delete(Options.LogFilePath + ".tmp");
        }
    }
}
