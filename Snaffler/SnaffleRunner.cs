using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using SnaffCore;
using SnaffCore.Concurrency;
using SnaffCore.Config;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading;
using System.ComponentModel;
using System.Web;

namespace Snaffler
{
    public class SnaffleRunner
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private BlockingMq Mq { get; set; }
        private LogLevel LogLevel { get; set; }
        private Options Options { get; set; }

        public void Run(string[] args)
        {
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

                if (Options == null)
                {
                    // bail out because the user was just running help
                    return;
                }

                //------------------------------------------
                // set up new fangled logging
                //------------------------------------------
                LoggingConfiguration nlogConfig = new LoggingConfiguration();
                nlogConfig.Variables["encoding"] = "utf8";
                TargetWithLayoutHeaderAndFooter logconsole = null;
                FileTarget logfile = null;

                ParseLogLevelString(Options.LogLevelString);

                // Targets where to log to: File and Console
                if (Options.LogToConsole)
                {
                    if (Options.NoColorLogs)
                    {
                        logconsole = new ConsoleTarget("logconsole");
                    }
                    else
                    {
                        logconsole = new ColoredConsoleTarget("logconsole")
                        {
                            DetectOutputRedirected = true,
                            UseDefaultRowHighlightingRules = false,
                            WordHighlightingRules =
                            {
                                new ConsoleWordHighlightingRule("[Green]", ConsoleOutputColor.Green,
                                    ConsoleOutputColor.Black),
                                new ConsoleWordHighlightingRule("[Yellow]", ConsoleOutputColor.Yellow,
                                    ConsoleOutputColor.Black),
                                new ConsoleWordHighlightingRule("[Red]", ConsoleOutputColor.Red,
                                    ConsoleOutputColor.Black),
                                new ConsoleWordHighlightingRule("[Black]", ConsoleOutputColor.White,
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
                                new ConsoleWordHighlightingRule("[File]", ConsoleOutputColor.DarkCyan,
                                    ConsoleOutputColor.Black),
                                new ConsoleWordHighlightingRule("[Share]", ConsoleOutputColor.Cyan,
                                    ConsoleOutputColor.Black),
                                new ConsoleWordHighlightingRule("[Dir]", ConsoleOutputColor.Blue,
                                    ConsoleOutputColor.Black),
                                new ConsoleWordHighlightingRule
                                {
                                    CompileRegex = true,
                                    Regex = @"<.*?>",
                                    ForegroundColor = ConsoleOutputColor.Cyan,
                                    BackgroundColor = ConsoleOutputColor.Black
                                }
                            }
                        };
                    }
                    
                    if (LogLevel == LogLevel.Warn)
                    {
                        nlogConfig.AddRule(LogLevel.Warn, LogLevel.Warn, logconsole);
                    }
                    else
                    {
                        nlogConfig.AddRule(LogLevel, LogLevel.Fatal, logconsole);
                    }
                    logconsole.Layout = "${message}";
                }

                if (Options.LogToFile)
                {
                    logfile = new FileTarget("logfile") { FileName = Options.LogFilePath };
                    if (LogLevel == LogLevel.Warn)
                    {
                        nlogConfig.AddRule(LogLevel.Warn, LogLevel.Warn, logfile);
                    }
                    else
                    {
                        nlogConfig.AddRule(LogLevel, LogLevel.Fatal, logfile);
                    }
                    if (Options.LogType == LogType.Plain)
                    {
                        logfile.Layout = "${message}";
                    }
                    else if (Options.LogType == LogType.JSON)
                    {
                        var eventProperties = new JsonLayout();
                        eventProperties.IncludeAllProperties = true;
                        eventProperties.MaxRecursionLimit = 5;
                        var jsonLayout = new JsonLayout
                        {
                            Attributes =
                            {
                                new JsonAttribute("time", "${longdate}"),
                                new JsonAttribute("level", "${level}"),
                                new JsonAttribute("message", "${message}"),
                                new JsonAttribute("eventProperties", eventProperties,
                                //don't escape layout
                                false)
                            }
                        };
                        logfile.Layout = jsonLayout;
                    }
                    else if (Options.LogType == LogType.HTML)
                    {
                        logfile.Layout = "<tr><td>${longdate}</td><td>${event-properties:htmlFields:objectPath=DateTime}</td><td>${level}</td><td>${event-properties:htmlFields:objectPath=Type}</td><td>${event-properties:htmlFields:objectPath=Triage}</td><td>${event-properties:htmlFields:objectPath=Permissions}</td><td>${event-properties:htmlFields:objectPath=Message}</td></tr>";
                    }
                }

                // Apply config           
                LogManager.Configuration = nlogConfig;

                //-------------------------------------------

                if (Options.Snaffle && (Options.SnafflePath.Length > 4))
                {
                    Directory.CreateDirectory(Options.SnafflePath);
                }

                controller = new SnaffCon(Options);

                var tokenSource = new CancellationTokenSource();
                var token = tokenSource.Token;
                Task thing = Task.Factory.StartNew(() => { controller.Execute(); }, token);
                bool exit = false;

                while (exit == false)
                {
                    if (HandleOutput() == true)
                    {
                        exit = true;
                    }
                }

                if (Options.LogType == LogType.JSON) FixJSONOutput();
                if (Options.LogType == LogType.HTML) FixHTMLOutput();

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
        }

        private bool HandleOutput()
        {
            BlockingMq Mq = BlockingMq.GetMq();
            foreach (SnafflerMessage message in Mq.Q.GetConsumingEnumerable())
            {
                ProcessMessage(message);

                // catch terminating messages and bail out of the master 'while' loop
                if ((message.Type == SnafflerMessageType.Fatal) || (message.Type == SnafflerMessageType.Finish))
                {
                    return true;
                }
            }
            return false;
        }

        private void ProcessMessage(SnafflerMessage message)
        {
            Logger messageLogger = Logger;
            string formattedMessageString;

            switch (Options.LogType)
            {
                case LogType.Plain:
                    formattedMessageString = message.ToString();
                    break;
                case LogType.JSON:
                    formattedMessageString = message.ToString();
                    messageLogger = messageLogger.WithProperty("SnafflerMessage", message);
                    break;
                case LogType.HTML:
                    SnafflerMessageComponents components = message.ToStringComponents();

                    formattedMessageString = SnafflerMessage.StringFromComponents(components);
                    messageLogger = messageLogger.WithProperty("htmlFields", new { components.DateTime, components.Type, components.Triage, components.Permissions, Message = HttpUtility.HtmlEncode(components.Message) });
                    break;
                default:
                    // this should be unreachable but whatever
                    formattedMessageString = message.ToString();
                    break;
            }

            switch (message.Type)
            {
                case SnafflerMessageType.Trace:
                    messageLogger.Trace(formattedMessageString);
                    break;
                case SnafflerMessageType.Degub:
                    messageLogger.Debug(formattedMessageString);
                    break;
                case SnafflerMessageType.Info:
                    messageLogger.Info(formattedMessageString);
                    break;
                case SnafflerMessageType.FileResult:
                case SnafflerMessageType.DirResult:
                case SnafflerMessageType.ShareResult:
                    messageLogger.Warn(formattedMessageString);
                    break;
                case SnafflerMessageType.Error:
                    messageLogger.Error(formattedMessageString);
                    break;
                case SnafflerMessageType.Fatal:
                    messageLogger.Fatal(formattedMessageString);
                    if (Debugger.IsAttached)
                    {
                        Console.ReadKey();
                    }
                    break;
                case SnafflerMessageType.Finish:
                    messageLogger.Info("Snaffler out.");
                    
                    if (Debugger.IsAttached)
                    {
                        Console.WriteLine("Press any key to exit.");
                        Console.ReadKey();
                    }

                    break;
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

        //This is probably slow but it is a quick and easy fix for now.
        private void FixJSONOutput() 
        {
            //Rename the log file temporarily
            File.Move(Options.LogFilePath, Options.LogFilePath + ".tmp");
            //Prepare the normalised file
            StreamWriter file = new StreamWriter(Options.LogFilePath);
            //Read in the original log file to an array
            string[] lines = System.IO.File.ReadAllLines(Options.LogFilePath + ".tmp");
            //Write the surrounding template that we need.
            file.Write("{\"entries\": [\n");
            //Write all the lines into the new file but add a comma after all but the last so it becomes valid JSON.
            for (int ii = 0; ii < lines.Length -1; ii++)
            {
                file.WriteLine(lines[ii] + ",");
            }
            //Add the last line but without a comma
            file.WriteLine(lines[lines.Length - 1]);
            //Close out the file's contents with the last of the JSON to make it valid.
            file.Write("]\n}");
            //Flush the output
            file.Flush();
            //Close the file
            file.Close();
            //Delete the temporary file.
            File.Delete(Options.LogFilePath + ".tmp");
        }

        private void FixHTMLOutput()
        {
            //Rename the log file temporarily
            File.Move(Options.LogFilePath, Options.LogFilePath + ".tmp");

            //Prepare the normalised file
            using (StreamWriter file = new StreamWriter(Options.LogFilePath))
            {
                //Write the start of the surrounding template that we need
                file.Write("<!doctypehtml><html lang=en><meta charset=UTF-8><meta content=\"width=device-width,initial-scale=1\"name=viewport><title>Snaffler Logs</title><style>table{border-collapse:collapse}td,th{border:2px solid #000;padding:5px;vertical-align:text-top}td:last-child{word-break: break-all}</style><div><table><thead><tr><th>Timestamp<th>DateTime<th>Level<th>Type<th>Triage<th>Permissions<th>Message<tbody>");

                //Open the original file
                using (FileStream sourceStream = new FileStream(Options.LogFilePath + ".tmp", FileMode.Open, FileAccess.Read))
                using (StreamReader sourceReader = new StreamReader(sourceStream))
                {
                    //Write the original content
                    file.Write(sourceReader.ReadToEnd());
                }

                //Write the end of the surrounding template that we need
                file.Write("</table></div>");
            }

            //Delete the temporary file
            File.Delete(Options.LogFilePath + ".tmp");
        }
    }
}
