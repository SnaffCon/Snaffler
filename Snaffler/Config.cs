using System;
using System.Linq;
using CommandLineParser.Arguments;
using Nett;
using NLog;
using SnaffCore.Concurrency;
using SnaffCore.Config;

namespace Snaffler
{
    public static class Config
    {
        public static Options Parse(string[] args)
        {
            BlockingMq Mq = BlockingMq.GetMq();
            Options options;

            // parse the args
            try
            {
                options = ParseImpl(args);
                if (options == null)
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
            return options;
        }

        private static Options ParseImpl(string[] args)
        {
            BlockingMq Mq = BlockingMq.GetMq();
            Mq.Info("Parsing args...");
            var retVal = new Options();

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
            var snaffleArg = new ValueArgument<string>('m', "snaffle",
                "Enables and assigns an output dir for Snaffler to automatically snaffle a copy of any found files.");
            var snaffleSizeArg = new ValueArgument<long>('l', "snafflesize", "Maximum size of file to snaffle, in bytes. Defaults to 10MB.");
            //var fileHuntArg = new SwitchArgument('f', "filehuntoff",
            //    "Disables file discovery, will only perform computer and share discovery.", false);
            var dirTargetArg = new ValueArgument<string>('i', "dirtarget",
                "Disables computer and share discovery, requires a path to a directory in which to perform file discovery.");
            var maxThreadsArg = new ValueArgument<int>('t', "threads", "Maximum number of threads. Default 30.");
            var domainArg = new ValueArgument<string>('d', "domain",
                "Domain to search for computers to search for shares on to search for files in. Easy.");
            var domainControllerArg = new ValueArgument<string>('c', "domaincontroller",
                "Domain controller to query for a list of domain computers.");
            var maxGrepSizeArg = new ValueArgument<long>('r', "maxgrepsize",
                "The maximum size file (in bytes) to search inside for interesting strings. Defaults to 500k.");
            var grepContextArg = new ValueArgument<int>('j', "grepcontext",
                "How many bytes of context either side of found strings in files to show, e.g. -j 200");

            // list of letters i haven't used yet: abefgknpquwxy

            var parser = new CommandLineParser.CommandLineParser();
            parser.Arguments.Add(configFileArg);
            parser.Arguments.Add(outFileArg);
            parser.Arguments.Add(helpArg);
            parser.Arguments.Add(stdOutArg);
            parser.Arguments.Add(snaffleArg);
            parser.Arguments.Add(snaffleSizeArg);
            parser.Arguments.Add(dirTargetArg);
            parser.Arguments.Add(maxThreadsArg);
            parser.Arguments.Add(domainArg);
            parser.Arguments.Add(verboseArg);
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

                var settings = TomlSettings.Create(cfg => cfg
                    .ConfigureType<LogLevel>(tc =>
                        tc.WithConversionFor<TomlString>(conv => conv
                            .FromToml(s => (LogLevel) Enum.Parse(typeof(LogLevel), s.Value, ignoreCase: true))
                            .ToToml(e => e.ToString()))));

                if (configFileArg.Parsed)
                {
                    if (!configFileArg.Value.Equals("generate"))
                    {
                        string configFile = configFileArg.Value;
                        retVal = Toml.ReadFile<Options>(configFile, settings);
                        retVal.PrepareClassifiers();
                        Mq.Info("Read config file from " + configFile);
                    }
                }


                if (retVal.ClassifierRules.Count <= 0)
                {
                    retVal.BuildDefaultClassifiers();
                }
                // get the args into our config

                // output args
                if (outFileArg.Parsed && (!String.IsNullOrEmpty(outFileArg.Value)))
                {
                    retVal.LogToFile = true;
                    retVal.LogFilePath = outFileArg.Value;
                    Mq.Degub("Logging to file at " + retVal.LogFilePath);
                }

                // Set loglevel.
                if (verboseArg.Parsed)
                {
                    retVal.LogLevelString = verboseArg.Value;
                    Mq.Degub("Requested verbosity level: " + retVal.LogLevelString);
                }

                // if enabled, display findings to the console
                retVal.LogToConsole = stdOutArg.Parsed;
                Mq.Degub("Enabled logging to stdout.");

                //if (maxThreadsArg.Parsed)
                //{
                //    retVal.MaxThreads = maxThreadsArg.Value;
                //    Mq.Degub("Max threads set to " + maxThreadsArg.Value);
                //}

                // args that tell us about targeting
                if ((domainArg.Parsed) && (!String.IsNullOrEmpty(domainArg.Value)))
                {
                    retVal.TargetDomain = domainArg.Value;
                    Mq.Degub("Target domain is " + domainArg.Value);
                }

                if ((domainControllerArg.Parsed) && (!String.IsNullOrEmpty(domainControllerArg.Value)))
                {
                    retVal.TargetDc = domainControllerArg.Value;
                    Mq.Degub("Target DC is " + domainControllerArg.Value);
                }

                if (dirTargetArg.Parsed)
                {
                    retVal.ShareFinderEnabled = false;
                    retVal.PathTargets = new string[] {dirTargetArg.Value};
                    Mq.Degub("Disabled finding shares.");
                    Mq.Degub("Target path is " + dirTargetArg.Value);
                }

                if (maxGrepSizeArg.Parsed)
                {
                    retVal.MaxSizeToGrep = maxGrepSizeArg.Value;
                    Mq.Degub("We won't bother looking inside files if they're bigger than " + retVal.MaxSizeToGrep +
                             " bytes");
                }

                if (snaffleArg.Parsed)
                {
                    retVal.SnafflePath = snaffleArg.Value;
                }

                if (snaffleSizeArg.Parsed)
                {
                    retVal.MaxSizeToSnaffle = snaffleSizeArg.Value;
                }

                // how many bytes 
                if (grepContextArg.Parsed)
                {
                    retVal.MatchContextBytes = grepContextArg.Value;
                    Mq.Degub(
                        "We'll show you " + grepContextArg.Value +
                        " bytes of context around matches inside files.");
                }

                // if enabled, grab a copy of files that we like.
                if (snaffleArg.Parsed)
                {
                    if (snaffleArg.Value.Length <= 0)
                    {
                        Mq.Error("-m or -mirror arg requires a path value.");
                        throw new ArgumentException("Invalid argument combination.");
                    }

                    retVal.Snaffle = true;
                    retVal.SnafflePath = snaffleArg.Value.TrimEnd('\\');
                    Mq.Degub("Mirroring matched files to path " + retVal.SnafflePath);
                }

                if (!retVal.LogToConsole && !retVal.LogToFile)
                {
                    Mq.Error(
                        "\nYou didn't enable output to file or to the console so you won't see any results or debugs or anything. Your l0ss.");
                    throw new ArgumentException("Pointless argument combination.");
                }

                if (configFileArg.Parsed)
                {
                    if (configFileArg.Value.Equals("generate"))
                    {
                        Toml.WriteFile(retVal, ".\\default.toml", settings);
                        Mq.Info("Wrote default config values to .\\default.toml");
                        Mq.Terminate();
                    }
                }

                retVal.PrepareClassifiers();
            }
            catch (Exception e)
            {
                Mq.Error(e.ToString());
                throw;
            }

            return retVal;
        }



    }
}