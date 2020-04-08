using System;
using System.Configuration;
using System.IO;
using System.Linq;
using CommandLineParser.Arguments;
using Nett;
using NLog;
using SnaffCore.Concurrency;
using Classifiers;

namespace SnaffCore.Config
{
    public partial class Config
    {
        public int MaxThreads { get; set; } = 30;
        public Options Options { get; set; }
        private static Config _instance;

        public static void Configure(string[] args)
        {
            _instance = new Config(args);
        }

        public static Config GetConfig()
        {
            return _instance;
        }

        private Config(string[] args)
        {
            this.Options = new Options();
            BlockingMq Mq = BlockingMq.GetMq();
            // parse the args
            try
            {
                var success = Parse(args);
                if (!success)
                {
                    throw new ArgumentException("Unable to correctly parse arguments.");
                }
            }
            catch
            {
                Mq.Error("Something went wrong parsing args.");
                throw;
            }

            Mq.Info("Parsed args successfully.");
        }

        public bool Parse(string[] args)
        {
            BlockingMq Mq = BlockingMq.GetMq();
            Mq.Info("Parsing args...");
            var success = false;

            // define args
            var configFileArg = new ValueArgument<string>('z', "config","Path to a .toml config file.");
            var outFileArg = new ValueArgument<string>('o', "outfile",
                "Path for output file. You probably want this if you're not using -s.");
            var verboseArg = new ValueArgument<string>('v', "verbosity",
                "Controls verbosity level, options are Trace (most verbose), Debug (less verbose), Info (less verbose still, default), and Data (results only). e.g '-v debug' ");
            var helpArg = new SwitchArgument('h', "help", "Displays this help.", false);
            var stdOutArg = new SwitchArgument('s', "stdout",
                "Enables outputting results to stdout as soon as they're found. You probably want this if you're not using -o.",
                false);
            var mirrorArg = new ValueArgument<string>('m', "mirror",
                "Enables and assigns an output dir for snaffler to automatically take a copy of any found files.");
            var fileHuntArg = new SwitchArgument('f', "filehuntoff",
                "Disables file discovery, will only perform computer and share discovery.", false);
            var dirTargetArg = new ValueArgument<string>('i', "dirtarget",
                "Disables computer and share discovery, requires a path to a directory in which to perform file discovery.");
            var maxThreadsArg = new ValueArgument<int>('t', "threads", "Maximum number of threads. Default 30.");
            var domainArg = new ValueArgument<string>('d', "domain",
                "Domain to search for computers to search for shares on to search for files in. Easy.");
            var adminshareArg = new SwitchArgument('a', "cdolla",
                "Enables scanning of C$ shares if found. Can be prone to false positives but more thorough.", false);
            var domainControllerArg = new ValueArgument<string>('c', "domaincontroller",
                "Domain controller to query for a list of domain computers.");
            var maxGrepSizeArg = new ValueArgument<long>('r', "maxgrepsize",
                "The maximum size file (in bytes) to search inside for interesting strings. Defaults to 500k.");
            var grepContextArg = new ValueArgument<int>('j', "grepcontext",
                "How many bytes of context either side of found strings in files to show, e.g. -j 200");

            // list of letters i haven't used yet: bklquwyz

            var parser = new CommandLineParser.CommandLineParser();

            parser.Arguments.Add(configFileArg);
            parser.Arguments.Add(outFileArg);
            parser.Arguments.Add(helpArg);
            //parser.Arguments.Add(extMatchArg);
            //parser.Arguments.Add(nameMatchArg);
            //parser.Arguments.Add(grepMatchArg);
            //parser.Arguments.Add(extSkipMatchArg);
            //parser.Arguments.Add(partialMatchArg);
            parser.Arguments.Add(stdOutArg);
            parser.Arguments.Add(mirrorArg);
            parser.Arguments.Add(fileHuntArg);
            parser.Arguments.Add(dirTargetArg);
            parser.Arguments.Add(maxThreadsArg);
            parser.Arguments.Add(domainArg);
            parser.Arguments.Add(verboseArg);
            parser.Arguments.Add(adminshareArg);
            parser.Arguments.Add(domainControllerArg);
            parser.Arguments.Add(maxGrepSizeArg);
            parser.Arguments.Add(grepContextArg);

            // extra check to handle builtin behaviour from cmd line arg parser
            if ((args.Contains("--help") || args.Contains("/?") || args.Contains("help") || args.Contains("-h") || args.Length == 0))
            {
                parser.ShowUsage();
                Environment.Exit(0);
            }


            try
            {
                parser.ParseCommandLine(args);

                if (configFileArg.Parsed)
                {
                    var settings = TomlSettings.Create(cfg => cfg
                        .ConfigureType<LogLevel>(tc =>
                            tc.WithConversionFor<TomlString>(conv => conv
                                .FromToml(s => (LogLevel)Enum.Parse(typeof(LogLevel), s.Value, ignoreCase: true))
                                .ToToml(e => e.ToString()))));
                    if (configFileArg.Value.Equals("generate"))
                    {
                        this.Options = new Options(true);
                        Toml.WriteFile(this.Options, ".\\default.toml", settings);
                        Mq.Info("Wrote default config values to .\\default.toml");
                        Mq.Terminate();
                    }
                    else
                    {
                        string configFile = configFileArg.Value;
                        this.Options = Toml.ReadFile<Options>(configFile, settings);
                        this.Options.PrepareClassifiers();
                        Mq.Info("Read config file from " + configFile);
                    }
                }
                else
                {

                    // get the args into our config


                    // output args
                    if (outFileArg.Parsed && (!String.IsNullOrEmpty(outFileArg.Value)))
                    {
                        Options.LogToFile = true;
                        Options.LogFilePath = outFileArg.Value;
                        Mq.Degub("Logging to file at " + Options.LogFilePath);
                    }

                    // Set loglevel.
                    if (verboseArg.Parsed)
                    {
                        var logLevelString = verboseArg.Value;
                        switch (logLevelString.ToLower())
                        {
                            case "debug":
                                Options.LogLevel = LogLevel.Debug;
                                Mq.Degub("Set verbosity level to degub.");
                                break;
                            case "degub":
                                Options.LogLevel = LogLevel.Debug;
                                Mq.Degub("Set verbosity level to degub.");
                                break;
                            case "trace":
                                Options.LogLevel = LogLevel.Trace;
                                Mq.Degub("Set verbosity level to trace.");
                                break;
                            case "data":
                                Options.LogLevel = LogLevel.Warn;
                                Mq.Degub("Set verbosity level to data.");
                                break;
                            default:
                                Options.LogLevel = LogLevel.Info;
                                Mq.Error("Invalid verbosity level " + logLevelString +
                                         " falling back to default level (info).");
                                break;
                        }
                    }

                    // if enabled, display findings to the console
                    Options.LogToConsole = stdOutArg.Parsed;
                    Mq.Degub("Enabled logging to stdout.");

                    if (maxThreadsArg.Parsed)
                    {
                        MaxThreads = maxThreadsArg.Value;
                        Mq.Degub("Max threads set to " + maxThreadsArg.Value);
                    }

                    // args that tell us about targeting
                    if ((domainArg.Parsed) && (!String.IsNullOrEmpty(domainArg.Value)))
                    {
                        Options.TargetDomain = domainArg.Value;
                        Mq.Degub("Target domain is " + domainArg.Value);
                    }

                    if ((domainControllerArg.Parsed) && (!String.IsNullOrEmpty(domainControllerArg.Value)))
                    {
                        Options.TargetDc = domainControllerArg.Value;
                        Mq.Degub("Target DC is " + domainControllerArg.Value);
                    }

                    if (dirTargetArg.Parsed)
                    {
                        Options.ShareFinderEnabled = false;
                        Options.DirTarget = dirTargetArg.Value;
                        Mq.Degub("Disabled finding shares.");
                        Mq.Degub("Target path is " + dirTargetArg.Value);
                    }

                    if (adminshareArg.Parsed)
                    {
                        Options.ScanCDollarShares = true;
                        Mq.Degub("Scanning of C$ shares enabled.");
                    }

                    // if the user passes the various MatchArgs with no value, that disables them.
                    // Otherwise load their wordlist into the appropriate config item.
                    /*
                    if (extMatchArg.Parsed)
                    {
                        if (extMatchArg.Value.Length <= 0)
                        {
                            ExactExtensionCheck = false;
                            Mq.Degub("Disabled matching based on exact file extension match.");
                        }
                        else
                        {
                            KeepExtExact = File.ReadAllLines(extMatchArg.Value);
                            Mq.Degub("Using file at " + extMatchArg.Value + " for exact file extension matching.");
                        }
                    }
    
                    if (pathMatchArg.Parsed)
                    {
                        if (pathMatchArg.Value.Length <= 0)
                        {
                            PartialPathCheck = false;
                            Mq.Degub("Disabled matching based on partial file path.");
                        }
                        else
                        {
                            KeepFilepathContains = File.ReadAllLines(pathMatchArg.Value);
                            Mq.Degub("Using file at " + pathMatchArg.Value + " for partial file path matching.");
                        }
                    }
    
                    if (extSkipMatchArg.Parsed)
                    {
                        if (extSkipMatchArg.Value.Length <= 0)
                        {
                            ExactExtensionSkipCheck = false;
                            Mq.Degub("Disabled skipping files with extensions on skip-list.");
                        }
                        else
                        {
                            DiscardExtExact = File.ReadAllLines(extSkipMatchArg.Value);
                            Mq.Degub("Using file at " + extSkipMatchArg.Value + " for extension skip-list.");
                        }
                    }
    
                    if (nameMatchArg.Parsed)
                    {
                        if (nameMatchArg.Value.Length <= 0)
                        {
                            ExactNameCheck = false;
                            Mq.Degub("Disabled matching based on exact file name");
                        }
                        else
                        {
                            KeepFilenameExact = File.ReadAllLines(nameMatchArg.Value);
                            Mq.Degub("Using file at " + nameMatchArg.Value + " for exact file name matching.");
                        }
                    }
    
                    if (grepMatchArg.Parsed)
                    {
                        if (grepMatchArg.Value.Length <= 0)
                        {
                            GrepByExtensionCheck = false;
                            Mq.Degub("Disabled matching based on file contents.");
                        }
                        else
                        {
                            GrepStrings = File.ReadAllLines(grepMatchArg.Value);
                            Mq.Degub("Using file at " + grepMatchArg.Value + " for file contents matching.");
                        }
                    }
    
                    if (partialMatchArg.Parsed)
                    {
                        if (partialMatchArg.Value.Length <= 0)
                        {
                            PartialNameCheck = false;
                            Mq.Degub("Disabled partial file name matching.");
                        }
                        else
                        {
                            NameStringsToKeep = File.ReadAllLines(partialMatchArg.Value);
                            Mq.Degub("Using file at " + partialMatchArg.Value + " for partial file name matching.");
                        }
                    }
                    */
                    if (maxGrepSizeArg.Parsed)
                    {
                        Options.MaxSizeToGrep = maxGrepSizeArg.Value;
                        Mq.Degub("We won't bother looking inside files if they're bigger than " + Options.MaxSizeToGrep +
                                 " bytes");
                    }

                    // how many bytes 
                    if (grepContextArg.Parsed)
                    {
                        Options.GrepContextBytes = grepContextArg.Value;
                        Mq.Degub(
                            "We'll show you " + grepContextArg.Value +
                            " bytes of context around matches inside files.");
                    }

                    // if enabled, grab a copy of files that we like.
                    if (mirrorArg.Parsed)
                    {
                        if (mirrorArg.Value.Length <= 0)
                        {
                            Mq.Error("-m or -mirror arg requires a path value.");
                            throw new ArgumentException("Invalid argument combination.");
                        }

                        Options.EnableMirror = true;
                        Options.MirrorPath = mirrorArg.Value.TrimEnd('\\');
                        Mq.Degub("Mirroring matched files to path " + Options.MirrorPath);
                    }

                    if (!Options.LogToConsole && !Options.LogToFile)
                    {
                        Mq.Error(
                            "\nYou didn't enable output to file or to the console so you won't see any results or debugs or anything. Your l0ss.");
                        throw new ArgumentException("Pointless argument combination.");
                    }
                }
            }
            catch (Exception e)
            {
                Mq.Error(e.ToString());
                throw;
            }


            success = true;
            return success;
        }
    }
}