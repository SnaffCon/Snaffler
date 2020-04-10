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

        public void Run(string[] args)
        {
            PrintBanner();
            BlockingMq.MakeMq();
            BlockingMq myMq = BlockingMq.GetMq();
            SnaffCon controller = null;
            try
            {
                Config.Configure(args);
                Config myConfig = Config.GetConfig();
                controller = new SnaffCon();
                //------------------------------------------
                // set up new fangled logging
                //------------------------------------------
                var nlogConfig = new LoggingConfiguration();

                ColoredConsoleTarget logconsole = null;
                FileTarget logfile = null;

                // Targets where to log to: File and Console
                if (myConfig.Options.LogToConsole)
                {
                    logconsole = new ColoredConsoleTarget("logconsole")
                    {
                        DetectOutputRedirected = true,
                        UseDefaultRowHighlightingRules = false,
                        WordHighlightingRules =
                        {
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
                                ForegroundColor = ConsoleOutputColor.Cyan
                            },
                            new ConsoleWordHighlightingRule
                            {
                                CompileRegex = true,
                                Regex = @"^\d\d\d\d-\d\d\-\d\d \d\d:\d\d:\d\d [\+-]\d\d:\d\d ",
                                ForegroundColor = ConsoleOutputColor.DarkGray
                            },
                            new ConsoleWordHighlightingRule
                            {
                                CompileRegex = true,
                                Regex = @"\((?:[^\)]*\)){1}",
                                ForegroundColor = ConsoleOutputColor.DarkMagenta
                            }
                        }
                    };
                    nlogConfig.AddRule(myConfig.Options.LogLevel, LogLevel.Fatal, logconsole);
                    logconsole.Layout = "${message}";
                }

                if (myConfig.Options.LogToFile)
                {
                    logfile = new FileTarget("logfile") {FileName = myConfig.Options.LogFilePath };
                    nlogConfig.AddRule(myConfig.Options.LogLevel, LogLevel.Fatal, logfile);
                    logfile.Layout = "${message}";
                }

                // Apply config           
                LogManager.Configuration = nlogConfig;

                //-------------------------------------------

                if (myConfig.Options.EnableMirror && (myConfig.Options.MirrorPath.Length > 4))
                {
                    Directory.CreateDirectory(myConfig.Options.MirrorPath);
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
            var isadmin = "";
            if (message.ShareResult.IsAdminShare)
            {
                isadmin = "AdminShare";
            }

            var shareResultTemplate = "<{0}>({1})";
            return string.Format(shareResultTemplate, isadmin, sharePath);
        }

        public string FileResultLogFromMessage(SnafflerMessage message)
        {
            var matchedclassifier = message.FileResult.MatchedClassifier.ClassifierName; //message.FileResult.WhyMatched.ToString();
            var triageString = message.FileResult.MatchedClassifier.Triage.ToString();

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

            var grepcontext = "";
            if (message.FileResult.GrepFileResult != null)
            {
                matchedstring = message.FileResult.GrepFileResult.GreppedStrings[0];
                grepcontext = message.FileResult.GrepFileResult.GrepContext;
            }

            var fileResultTemplate = "<{0}|{1}{2}|{3}|{4}|{5}>({6}) {7}";
            return string.Format(fileResultTemplate, matchedclassifier, canread, canwrite, matchedstring, fileSizeString, triageString,
                filepath, grepcontext);
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

        /*
public void MirrorMessageFile(FileInfo fileInfo)
{
    // do the mirror dance
    // TODO this should 100% be split off into its own task.
    string mirrored = "";
    if (message.Mirror && message.FileResult.RWStatus.canRead)
    {

        if (fileSize < 500000)
        {
            string sourcePath = message.FileResult.FileInfo.FullName;
            // clean it up and normalise it a bit
            string cleanedPath = Path.GetFullPath(sourcePath.Replace(':', '.').Replace('$', '.'));
            // make the dir exist
            Directory.CreateDirectory(Path.GetDirectoryName(Config.MirrorPath + cleanedPath));

            File.Copy(sourcePath, (Path.GetFullPath(Config.MirrorPath + cleanedPath)), true);
            mirrored = "Mirrored";
        }
        else
        {
            mirrored = "Skipped";
        }
    }
}
*/

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