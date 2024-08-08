using CommandLineParser.Arguments;
using Nett;
using NLog;
using SnaffCore.Concurrency;
using SnaffCore.Config;
using System;
using System.Resources;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Security;
using System.Collections.Generic;
using System.Net;

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
                    return null;
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


        public static string ReadResource(string name)
        {
            // Determine path
            var assembly = Assembly.GetExecutingAssembly();
            string resourcePath = name;
            // Format: "{Namespace}.{Folder}.{filename}.{Extension}"

            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static bool isIP(string host)
        {
            IPAddress ip;
            return IPAddress.TryParse(host, out ip);
        }

        private static Options ParseImpl(string[] args)
        {
            BlockingMq Mq = BlockingMq.GetMq();
            Mq.Info("Parsing args...");
            Options parsedConfig = new Options();

            // define args
            ValueArgument<string> configFileArg = new ValueArgument<string>('z', "config", "Path to a .toml config file. Run with \'generate\' to puke a sample config file into the working directory.");
            ValueArgument<string> outFileArg = new ValueArgument<string>('o', "outfile",
                "Path for output file. You probably want this if you're not using -s.");
            ValueArgument<string> verboseArg = new ValueArgument<string>('v', "verbosity",
                "Controls verbosity level, options are Trace (most verbose), Debug (less verbose), Info (less verbose still, default), and Data (results only). e.g '-v debug' ");
            SwitchArgument helpArg = new SwitchArgument('h', "help", "Displays this help.", false);
            SwitchArgument stdOutArg = new SwitchArgument('s', "stdout",
                "Enables outputting results to stdout as soon as they're found. You probably want this if you're not using -o.",
                false);
            ValueArgument<int> interestLevel = new ValueArgument<int>('b', "interest", "Interest level to report (0-3)");
            ValueArgument<string> snaffleArg = new ValueArgument<string>('m', "snaffle",
                "Enables and assigns an output dir for Snaffler to automatically snaffle a copy of any found files.");
            ValueArgument<long> snaffleSizeArg = new ValueArgument<long>('l', "snafflesize", "Maximum size of file to snaffle, in bytes. Defaults to 10MB.");
            //var fileHuntArg = new SwitchArgument('f', "filehuntoff",
            //    "Disables file discovery, will only perform computer and share discovery.", false);
            ValueArgument<string> dirTargetArg = new ValueArgument<string>('i', "dirtarget",
                "Disables computer and share discovery, requires a path to a directory in which to perform file discovery.");
            ValueArgument<string> domainArg = new ValueArgument<string>('d', "domain",
                "Domain to search for computers to search for shares on to search for files in. Easy.");
            ValueArgument<string> domainControllerArg = new ValueArgument<string>('c', "domaincontroller",
                "Domain controller to query for a list of domain computers.");
            ValueArgument<long> maxGrepSizeArg = new ValueArgument<long>('r', "maxgrepsize",
                "The maximum size file (in bytes) to search inside for interesting strings. Defaults to 500k.");
            ValueArgument<int> grepContextArg = new ValueArgument<int>('j', "grepcontext",
                "How many bytes of context either side of found strings in files to show, e.g. -j 200");
            SwitchArgument domainUserArg = new SwitchArgument('u', "domainusers", "Makes Snaffler grab a list of interesting-looking accounts from the domain and uses them in searches.", false);
            ValueArgument<int> maxThreadsArg = new ValueArgument<int>('x', "maxthreads", "How many threads to be snaffling with. Any less than 4 and you're gonna have a bad time.");
            SwitchArgument tsvArg = new SwitchArgument('y', "tsv", "Makes Snaffler output as tsv.", false);
            SwitchArgument dfsArg = new SwitchArgument('f', "dfs", "Limits Snaffler to finding file shares via DFS, for \"OPSEC\" reasons.", false);
            SwitchArgument findSharesOnlyArg = new SwitchArgument('a', "sharesonly",
                "Stops after finding shares, doesn't walk their filesystems.", false);
            ValueArgument<string> compExclusionArg = new ValueArgument<string>('k', "exclusions", "Path to a file containing a list of computers to exclude from scanning.");
            ValueArgument<string> compTargetArg = new ValueArgument<string>('n', "comptarget", "Computer (or comma separated list) to target.");
            ValueArgument<string> ruleDirArg = new ValueArgument<string>('p', "rulespath", "Path to a directory full of toml-formatted rules. Snaffler will load all of these in place of the default ruleset.");
            ValueArgument<string> logType = new ValueArgument<string>('t', "logtype", "Type of log you would like to output. Currently supported options are plain and JSON. Defaults to plain.");
            ValueArgument<string> timeOutArg = new ValueArgument<string>('e', "timeout",
                "Interval between status updates (in minutes) also acts as a timeout for AD data to be gathered via LDAP. Turn this knob up if you aren't getting any computers from AD when you run Snaffler through a proxy or other slow link. Default = 5");
            // list of letters i haven't used yet: gnqw

            CommandLineParser.CommandLineParser parser = new CommandLineParser.CommandLineParser();
            parser.Arguments.Add(timeOutArg);
            parser.Arguments.Add(configFileArg);
            parser.Arguments.Add(outFileArg);
            parser.Arguments.Add(helpArg);
            parser.Arguments.Add(stdOutArg);
            parser.Arguments.Add(snaffleArg);
            parser.Arguments.Add(snaffleSizeArg);
            parser.Arguments.Add(dirTargetArg);
            parser.Arguments.Add(interestLevel);
            parser.Arguments.Add(domainArg);
            parser.Arguments.Add(verboseArg);
            parser.Arguments.Add(domainControllerArg);
            parser.Arguments.Add(maxGrepSizeArg);
            parser.Arguments.Add(grepContextArg);
            parser.Arguments.Add(domainUserArg);
            parser.Arguments.Add(tsvArg);
            parser.Arguments.Add(dfsArg);
            parser.Arguments.Add(findSharesOnlyArg);
            parser.Arguments.Add(maxThreadsArg);
            parser.Arguments.Add(compTargetArg);
            parser.Arguments.Add(ruleDirArg);
            parser.Arguments.Add(logType);
            parser.Arguments.Add(compExclusionArg);

            // extra check to handle builtin behaviour from cmd line arg parser
            if ((args.Contains("--help") || args.Contains("/?") || args.Contains("help") || args.Contains("-h") || args.Length == 0))
            {
                parser.ShowUsage();
                return null; 
            }

            TomlSettings settings = TomlSettings.Create(cfg => cfg
.ConfigureType<LogLevel>(tc =>
    tc.WithConversionFor<TomlString>(conv => conv
        .FromToml(s => (LogLevel)Enum.Parse(typeof(LogLevel), s.Value, ignoreCase: true))
        .ToToml(e => e.ToString()))));

            try
            {
                parser.ParseCommandLine(args);

                if (timeOutArg.Parsed && !String.IsNullOrWhiteSpace(timeOutArg.Value))
                {
                    int timeOutVal;
                    if (int.TryParse(timeOutArg.Value, out timeOutVal))
                    {
                        Mq.Info("Set timeout/update interval to " + timeOutVal.ToString() + " minutes.");
                        parsedConfig.TimeOut = timeOutVal;
                    }
                    else
                    {
                        Mq.Error("Invalid timeout value passed, defaulting to 5 mins.");
                    }
                }

                if (logType.Parsed && !String.IsNullOrWhiteSpace(logType.Value))
                {
                    //Set the default to plain
                    parsedConfig.LogType = LogType.Plain;
                    //if they set a different type then replace it with the new type.
                    if (logType.Value.ToLower() == "json")
                    {
                        parsedConfig.LogType = LogType.JSON;
                    }
                    else
                    {
                        Mq.Info("Invalid type argument passed (" + logType.Value + ") defaulting to plaintext");
                    }
                }

                if (ruleDirArg.Parsed && !String.IsNullOrWhiteSpace(ruleDirArg.Value))
                {
                    parsedConfig.RuleDir = ruleDirArg.Value;
                }

                // get the args into our config

                // output args
                if (outFileArg.Parsed && (!String.IsNullOrEmpty(outFileArg.Value)))
                {
                    parsedConfig.LogToFile = true;
                    parsedConfig.LogFilePath = outFileArg.Value;
                    Mq.Degub("Logging to file at " + parsedConfig.LogFilePath);
                }

                if (dfsArg.Parsed)
                {
                    parsedConfig.DfsOnly = dfsArg.Value;
                }
                if (compExclusionArg.Parsed)
                {
                    List<string> compExclusions = new List<string>();
                    string[] fileLines = File.ReadAllLines(compExclusionArg.Value);
                    foreach (string line in fileLines)
                    {
                        if (isIP(line))
                        {
                            compExclusions.Add(line);
                        }
                        else
                        {
                            try
                            {
                                IPHostEntry result = Dns.GetHostEntry(line);
                                foreach (IPAddress ipAddress in result.AddressList)
                                {
                                    compExclusions.Add(ipAddress.ToString());
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                                continue;
                            }
                        }
                    }
                    if (compExclusions.Count > 0)
                    {
                        parsedConfig.ComputerExclusions = compExclusions;
                        parsedConfig.ComputerExclusionFile = compExclusionArg.Value;
                    }
                    else
                    {
                        throw new Exception("Failed to get a valid list of excluded computers from the excluded computers list.");
                    }
                }
                if (compTargetArg.Parsed)
                {
                    string[] compTargets = null;
                    if (compTargetArg.Value.Contains(","))
                    {
                        compTargets = compTargetArg.Value.Split(',');
                        
                    }
                    else
                    {
                        compTargets = new string[] { compTargetArg.Value };
                    }
                    parsedConfig.ComputerTargets = compTargets;
                }

                if (findSharesOnlyArg.Parsed)
                {
                    parsedConfig.ScanFoundShares = false;
                }
                if (maxThreadsArg.Parsed)
                {
                    parsedConfig.MaxThreads = maxThreadsArg.Value;
                }

                parsedConfig.ShareThreads = parsedConfig.MaxThreads / 3;
                parsedConfig.FileThreads = parsedConfig.MaxThreads / 3;
                parsedConfig.TreeThreads = parsedConfig.MaxThreads / 3;

                if (tsvArg.Parsed)
                {
                    parsedConfig.LogTSV = true;
                    if (parsedConfig.Separator == ' ')
                    {
                        parsedConfig.Separator = '\t';
                    }
                }

                // Set loglevel.
                if (verboseArg.Parsed)
                {
                    parsedConfig.LogLevelString = verboseArg.Value;
                    Mq.Degub("Requested verbosity level: " + parsedConfig.LogLevelString);
                }

                // if enabled, display findings to the console
                parsedConfig.LogToConsole = stdOutArg.Parsed;
                Mq.Degub("Enabled logging to stdout.");

                // args that tell us about targeting
                if ((domainArg.Parsed) && (!String.IsNullOrEmpty(domainArg.Value)))
                {
                    parsedConfig.TargetDomain = domainArg.Value;
                    Mq.Degub("Target domain is " + domainArg.Value);
                }

                if ((domainControllerArg.Parsed) && (!String.IsNullOrEmpty(domainControllerArg.Value)))
                {
                    parsedConfig.TargetDc = domainControllerArg.Value;
                    Mq.Degub("Target DC is " + domainControllerArg.Value);
                }

                if (domainUserArg.Parsed)
                {
                    parsedConfig.DomainUserRules = true;
                    Mq.Degub("Enabled use of domain user accounts in rules.");
                }

                if (dirTargetArg.Parsed)
                {
                    parsedConfig.ShareFinderEnabled = false;
                    //Console.WriteLine(dirTargetArg.Value);
                    string pathTarget = dirTargetArg.Value;
                    if (dirTargetArg.Value.Length > 4)
                    {
                        pathTarget = dirTargetArg.Value.TrimEnd('\\');
                    }
                    parsedConfig.PathTargets.Add(pathTarget);
                    Console.WriteLine(parsedConfig.PathTargets[0]);
                    Mq.Degub("Disabled finding shares.");
                    Mq.Degub("Target path is " + dirTargetArg.Value);
                }

                if (maxGrepSizeArg.Parsed)
                {
                    parsedConfig.MaxSizeToGrep = maxGrepSizeArg.Value;
                    Mq.Degub("We won't bother looking inside files if they're bigger than " + parsedConfig.MaxSizeToGrep +
                             " bytes");
                }

                if (snaffleSizeArg.Parsed)
                {
                    parsedConfig.MaxSizeToSnaffle = snaffleSizeArg.Value;
                }

                if (interestLevel.Parsed)
                {
                    parsedConfig.InterestLevel = interestLevel.Value;
                    Mq.Degub("Requested interest level: " + parsedConfig.InterestLevel);
                }

                // how many bytes 
                if (grepContextArg.Parsed)
                {
                    parsedConfig.MatchContextBytes = grepContextArg.Value;
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

                    parsedConfig.Snaffle = true;
                    parsedConfig.SnafflePath = snaffleArg.Value.TrimEnd('\\');
                    Mq.Degub("Mirroring matched files to path " + parsedConfig.SnafflePath);
                }

                if (configFileArg.Parsed)
                {
                    if (configFileArg.Value.Equals("generate"))
                    {
                        Toml.WriteFile(parsedConfig, ".\\default.toml", settings);
                        Console.WriteLine("Wrote config values to .\\default.toml");
                        parsedConfig.LogToConsole = true;
                        Mq.Degub("Enabled logging to stdout.");
                        return null;
                    }
                    else
                    {
                        string configFile = configFileArg.Value;
                        parsedConfig = Toml.ReadFile<Options>(configFile, settings);
                        Mq.Info("Read config file from " + configFile);
                    }
                }

                if (!parsedConfig.LogToConsole && !parsedConfig.LogToFile)
                {
                    Mq.Error(
                        "\nYou didn't enable output to file or to the console so you won't see any results or debugs or anything. Your l0ss.");
                    throw new ArgumentException("Pointless argument combination.");
                }

                if (parsedConfig.ClassifierRules.Count <= 0)
                {
                    if (String.IsNullOrWhiteSpace(parsedConfig.RuleDir))
                        {
                        // get all the embedded toml file resources
                        string[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                        StringBuilder sb = new StringBuilder();

                        foreach (string resourceName in resourceNames)
                        {
                            if (!resourceName.EndsWith(".toml"))
                            {
                                // skip this one as it's just metadata
                                continue;
                            }
                            string ruleFile = ReadResource(resourceName);
                            sb.AppendLine(ruleFile);
                        }

                        string bulktoml = sb.ToString();

                        // deserialise the toml to an actual ruleset
                        RuleSet ruleSet = Toml.ReadString<RuleSet>(bulktoml, settings);

                        // stick the rules in our config!
                        parsedConfig.ClassifierRules = ruleSet.ClassifierRules;
                    }
                    else
                    {
                        string[] tomlfiles = Directory.GetFiles(parsedConfig.RuleDir, "*.toml", SearchOption.AllDirectories);
                        StringBuilder sb = new StringBuilder();
                        foreach (string tomlfile in tomlfiles)
                        {
                            string tomlstring = File.ReadAllText(tomlfile);
                            sb.AppendLine(tomlstring);
                        }
                        string bulktoml = sb.ToString();
                        // deserialise the toml to an actual ruleset
                        RuleSet ruleSet = Toml.ReadString<RuleSet>(bulktoml, settings);

                        // stick the rules in our config!
                        parsedConfig.ClassifierRules = ruleSet.ClassifierRules;
                    }
                }

                parsedConfig.PrepareClassifiers();
            }
            catch (Exception e)
            {
                Mq.Error(e.ToString());
                throw;
            }

            return parsedConfig;
        }



    }
}
