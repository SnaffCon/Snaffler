using NLog;
using NLog.Config;
using NLog.Targets;
using SnaffCore;
using SnaffCore.Concurrency;
using SnaffCore.Config;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Snaffler
{
    public class SnaffleRunner
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
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

        public void Run(string[] args)
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
                Options = Config.Parse(args);

                // set up the  TSV output if the flag is set
                if (Options.LogTSV)
                {
                    fileResultTemplate = "{0}" + Options.Separator + "{1}" + Options.Separator + "{2}" + Options.Separator + "{3}" + Options.Separator + "{4}" + Options.Separator + "{5}" + Options.Separator + "{6:u}" + Options.Separator + "{7}" + Options.Separator + "{8}";
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
                // set up new fangled logging
                //------------------------------------------
                LoggingConfiguration nlogConfig = new LoggingConfiguration();

                ColoredConsoleTarget logconsole = null;
                FileTarget logfile = null;

                ParseLogLevelString(Options.LogLevelString);

                // Targets where to log to: File and Console
                if (Options.LogToConsole)
                {
                    logconsole = new ColoredConsoleTarget("logconsole")
                    {
                        DetectOutputRedirected = true,
                        UseDefaultRowHighlightingRules = false,
                        WordHighlightingRules =
                        {
                            new ConsoleWordHighlightingRule("{Green}", ConsoleOutputColor.DarkGreen,
                                ConsoleOutputColor.White),
                            new ConsoleWordHighlightingRule("{Yellow}", ConsoleOutputColor.DarkYellow,
                                ConsoleOutputColor.White),
                            new ConsoleWordHighlightingRule("{Red}", ConsoleOutputColor.DarkRed,
                                ConsoleOutputColor.White),
                            new ConsoleWordHighlightingRule("{Black}", ConsoleOutputColor.Black,
                                ConsoleOutputColor.White),

                            new ConsoleWordHighlightingRule("[Trace]", ConsoleOutputColor.DarkGray,
                                ConsoleOutputColor.Black),
                            new ConsoleWordHighlightingRule("[Degub]", ConsoleOutputColor.Gray,
                                ConsoleOutputColor.Black),
                            new ConsoleWordHighlightingRule("[Info]", ConsoleOutputColor.White,
                                ConsoleOutputColor.Black),
                            new ConsoleWordHighlightingRule("[Error]", ConsoleOutputColor.Magenta,
                                ConsoleOutputColor.Black),
                            new ConsoleWordHighlightingRule("[Fatal]", ConsoleOutputColor.Red,
                                ConsoleOutputColor.Black),
                            new ConsoleWordHighlightingRule("[File]", ConsoleOutputColor.Green,
                                ConsoleOutputColor.Black),
                            new ConsoleWordHighlightingRule("[Share]", ConsoleOutputColor.Yellow,
                                ConsoleOutputColor.Black),
                            new ConsoleWordHighlightingRule
                            {
                                CompileRegex = true,
                                Regex = @"<.*\|.*\|.*\|.*?>",
                                ForegroundColor = ConsoleOutputColor.Cyan,
                                BackgroundColor = ConsoleOutputColor.Black
                            },
                            new ConsoleWordHighlightingRule
                            {
                                CompileRegex = true,
                                Regex = @"^\d\d\d\d-\d\d\-\d\d \d\d:\d\d:\d\d [\+-]\d\d:\d\d ",
                                ForegroundColor = ConsoleOutputColor.DarkGray,
                                BackgroundColor = ConsoleOutputColor.Black
                            },
                            new ConsoleWordHighlightingRule
                            {
                                CompileRegex = true,
                                Regex = @"\((?:[^\)]*\)){1}",
                                ForegroundColor = ConsoleOutputColor.DarkMagenta,
                                BackgroundColor = ConsoleOutputColor.Black
                            }
                        }
                    };
                    nlogConfig.AddRule(LogLevel, LogLevel.Fatal, logconsole);
                    logconsole.Layout = "${message}";
                }

                if (Options.LogToFile)
                {
                    logfile = new FileTarget("logfile") { FileName = Options.LogFilePath };
                    nlogConfig.AddRule(LogLevel, LogLevel.Fatal, logfile);
                    logfile.Layout = "${message}";
                }

                // Apply config           
                LogManager.Configuration = nlogConfig;

                //-------------------------------------------

                if (Options.Snaffle && (Options.SnafflePath.Length > 4))
                {
                    Directory.CreateDirectory(Options.SnafflePath);
                }

                controller = new SnaffCon(Options);
                Task thing = Task.Factory.StartNew(() => { controller.Execute(); });

                while (true)
                {
                    HandleOutput();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                DumpQueue();
            }
        }

        private void DumpQueue()
        {
            BlockingMq Mq = BlockingMq.GetMq();
            while (Mq.Q.TryTake(out SnafflerMessage message))
            {
                // emergency dump of queue contents to console
                Console.WriteLine(message.Message);
            }
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.ReadKey();
            }
            Environment.Exit(1);
        }

        private void HandleOutput()
        {
            BlockingMq Mq = BlockingMq.GetMq();
            foreach (SnafflerMessage message in Mq.Q.GetConsumingEnumerable())
            {
                ProcessMessage(message);
            }
        }

        private void ProcessMessage(SnafflerMessage message)
        {
            //  standardized time formatting
            string datetime =  String.Format("{1}{0}{2:u}{0}",  Options.Separator,hostString(), message.DateTime);

            switch (message.Type)
            {
                case SnafflerMessageType.Trace:
                    Logger.Trace(datetime + "[Trace]" + Options.Separator + message.Message);
                    break;
                case SnafflerMessageType.Degub:
                    Logger.Debug(datetime + "[Degub]" + Options.Separator + message.Message);
                    break;
                case SnafflerMessageType.Info:
                    Logger.Info(datetime + "[Info]" + Options.Separator + message.Message);
                    break;
                case SnafflerMessageType.FileResult:
                    Logger.Warn(datetime + "[File]" + Options.Separator + FileResultLogFromMessage(message));
                    break;
                case SnafflerMessageType.DirResult:
                    Logger.Warn(datetime + "[Dir]" + Options.Separator + DirResultLogFromMessage(message));
                    break;
                case SnafflerMessageType.ShareResult:
                    Logger.Warn(datetime + "[Share]" + Options.Separator + ShareResultLogFromMessage(message));
                    break;
                case SnafflerMessageType.Error:
                    Logger.Error(datetime + "[Error]" + Options.Separator + message.Message);
                    break;
                case SnafflerMessageType.Fatal:
                    Logger.Fatal(datetime + "[Fatal]" + Options.Separator + message.Message);
                    if (Debugger.IsAttached)
                    {
                        Console.ReadKey();
                    }
                    Environment.Exit(1);
                    break;
                case SnafflerMessageType.Finish:
                    Logger.Info("Snaffler out.");
                    Console.WriteLine("Press any key to exit.");
                    if (Debugger.IsAttached)
                    {
                        Console.ReadKey();
                    }
                    Environment.Exit(0);
                    break;
            }
        }

        public string ShareResultLogFromMessage(SnafflerMessage message)
        {
            string sharePath = message.ShareResult.SharePath;
            string triage = message.ShareResult.Triage.ToString();
            string shareComment = message.ShareResult.ShareComment;

            string rwString = "";
            string canread = "";
            if (message.ShareResult.RootReadable)
            {
                rwString = rwString + "R";
            }

            string canwrite = "";
            if (message.ShareResult.RootWritable)
            {
                rwString = rwString + "W";
            }

            string canmodify = "";
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
            Console.ForegroundColor = fgColor;

            Console.Write(textToWrite);

            Console.ResetColor();
        }

        public void WriteColorLine(string textToWrite, ConsoleColor fgColor)
        {
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
    }
}