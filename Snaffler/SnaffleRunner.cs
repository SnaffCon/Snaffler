using System;
using System.IO;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Targets;
using SnaffCore;
using SnaffCore.Concurrency;
using SnaffCore.Config;

namespace Snaffler
{
    public class SnaffleRunner
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private BlockingMq Mq { get; set; }

        public void Run(string[] args)
        {
            PrintBanner();
            BlockingMq.MakeMq();
            Mq = BlockingMq.GetMq();
            SnaffCon controller = null;
            Options myOptions;

            try
            {
                myOptions = Config.Parse(args);
                controller = new SnaffCon();

                //------------------------------------------
                // set up new fangled logging
                //------------------------------------------
                var nlogConfig = new LoggingConfiguration();

                ColoredConsoleTarget logconsole = null;
                FileTarget logfile = null;

                // Targets where to log to: File and Console
                if (myOptions.LogToConsole)
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
                    nlogConfig.AddRule(myOptions.LogLevel, LogLevel.Fatal, logconsole);
                    logconsole.Layout = "${message}";
                }

                if (myOptions.LogToFile)
                {
                    logfile = new FileTarget("logfile") {FileName = myOptions.LogFilePath };
                    nlogConfig.AddRule(myOptions.LogLevel, LogLevel.Fatal, logfile);
                    logfile.Layout = "${message}";
                }

                // Apply config           
                LogManager.Configuration = nlogConfig;

                //-------------------------------------------

                if (myOptions.Snaffle && (myOptions.SnafflePath.Length > 4))
                {
                    Directory.CreateDirectory(myOptions.SnafflePath);
                }

                var thing = Task.Factory.StartNew(() => { controller.Execute(); });

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
            while (Mq.Q.TryTake(out var message))
            {
                // emergency dump of queue contents to console
                Console.WriteLine(message.Message);
            }

            Environment.Exit(1);
        }

        private void HandleOutput()
        {
            BlockingMq Mq = BlockingMq.GetMq();
            foreach (var message in Mq.Q.GetConsumingEnumerable())
            {
                ProcessMessage(message);
            }
        }

        private void ProcessMessage(SnafflerMessage message)
        {
            var datetime = message.DateTime.ToString("yyyy-MM-dd HH:mm:ss zzz ");
            switch (message.Type)
            {
                case SnafflerMessageType.Trace:
                    Logger.Trace(datetime + "[Trace] " + message.Message);
                    break;
                case SnafflerMessageType.Degub:
                    Logger.Debug(datetime + "[Degub] " + message.Message);
                    break;
                case SnafflerMessageType.Info:
                    Logger.Info(datetime + "[Info] " + message.Message);
                    break;
                case SnafflerMessageType.FileResult:
                    Logger.Warn(datetime + "[File]" + FileResultLogFromMessage(message));
                    break;
                case SnafflerMessageType.ShareResult:
                    Logger.Warn(datetime + "[Share]" + ShareResultLogFromMessage(message));
                    break;
                case SnafflerMessageType.Error:
                    Logger.Error(datetime + "[Error] " + message.Message);
                    break;
                case SnafflerMessageType.Fatal:
                    Logger.Fatal(datetime + "[Fatal] " + message.Message);
                    Environment.Exit(1);
                    break;
            }
        }

        public string ShareResultLogFromMessage(SnafflerMessage message)
        {
            var sharePath = message.ShareResult.SharePath;
            var triage = message.ShareResult.Triage.ToString();
            var shareResultTemplate = "{{0}}({1})";
            return string.Format(shareResultTemplate, triage, sharePath);
        }

        public string FileResultLogFromMessage(SnafflerMessage message)
        {
            try
            {
                var matchedclassifier = message.FileResult.MatchedRule.RuleName; //message.FileResult.WhyMatched.ToString();
                var triageString = message.FileResult.MatchedRule.Triage.ToString();

                var canread = "";
                if (message.FileResult.RwStatus.CanRead)
                {
                    canread = "R";
                }

                var canwrite = "";
                if (message.FileResult.RwStatus.CanWrite)
                {
                    canwrite = "W";
                }

                var matchedstring = "";

                var fileSize = message.FileResult.FileInfo.Length;
                var fileSizeString = BytesToString(fileSize);

                var filepath = message.FileResult.FileInfo.FullName;

                var matchcontext = "";
                if (message.FileResult.TextResult != null)
                {
                    matchedstring = message.FileResult.TextResult.MatchedStrings[0];
                    matchcontext = message.FileResult.TextResult.MatchContext;
                }

                var fileResultTemplate = " {{{0}}}<{1}|{2}{3}|{4}|{5}>({6}) {7}";
                return string.Format(fileResultTemplate, triageString, matchedclassifier, canread, canwrite, matchedstring, fileSizeString,
                    filepath, matchcontext);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine(message.FileResult.FileInfo.FullName);
                return "";
            }
        }

        private static String BytesToString(long byteCount)
        {
            string[] suf = {"B", "kB", "MB", "GB", "TB", "PB", "EB"}; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);
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
            var barfLines = new[]
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

            var i = 0;
            foreach (var barfLine in barfLines)
            {
                var barfOne = barfLine;
                WriteColorLine(barfOne, patternOne[i]);
                i += 1;
            }

            Console.WriteLine("\n");
        }
    }
}