using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Dispatcher;
using System.Text.RegularExpressions;
using SnaffCore.Concurrency;
using SnaffCore.Config;

namespace Classifiers
{
    public class TextClassifier
    {
        private ClassifierRule ClassifierRule { get; set; }
        public TextClassifier(ClassifierRule inRule)
        {
            this.ClassifierRule = inRule;
        }

        private Config myConfig { get; set; } = Config.GetConfig();

        private BlockingMq Mq { get; set; } = BlockingMq.GetMq();

        // Methods for classification
        internal TextResult TextMatch(string input)
        {
            // generic match checking
            switch (ClassifierRule.WordListType)
            {
                case MatchListType.Contains:
                    foreach (string matchString in ClassifierRule.WordList)
                    {
                        if (input.ToLower().Contains(matchString.ToLower()))
                        {
                            return new TextResult()
                            {
                                MatchedStrings = new List<string>(){matchString},
                                MatchContext = GetContext(input, matchString)
                            };
                        }
                    }

                    break;
                case MatchListType.EndsWith:
                    foreach (string matchString in ClassifierRule.WordList)
                    {
                        if (input.ToLower().EndsWith(matchString.ToLower()))
                        {
                            return new TextResult()
                            {
                                MatchedStrings = new List<string>() {matchString},
                                MatchContext = GetContext(input, matchString)
                            };
                        }
                    }

                    break;
                case MatchListType.Exact:
                    foreach (string matchString in ClassifierRule.WordList)
                    {
                        if (input.ToLower() == matchString.ToLower())
                        {
                            return new TextResult()
                            {
                                MatchedStrings = new List<string>() { matchString },
                                MatchContext = GetContext(input, matchString)
                            };
                        }
                    }

                    break;
                case MatchListType.StartsWith:
                    foreach (string matchString in ClassifierRule.WordList)
                    {
                        if (input.ToLower().StartsWith(matchString.ToLower()))
                        {
                            return new TextResult()
                            {
                                MatchedStrings = new List<string>() { matchString },
                                MatchContext = GetContext(input, matchString)
                            };
                        }
                    }

                    break;
                case MatchListType.Regex:
                    foreach (string matchString in ClassifierRule.WordList)
                    {
                        Regex regex = new Regex(matchString);

                        if (regex.IsMatch(input))
                        {
                            return new TextResult()
                            {
                                MatchedStrings = new List<string>() { matchString },
                                MatchContext = GetContext(input, regex)
                            };
                        }
                    }
                    break;
                default:
                    return null;
            }
            return null;
        }

        internal string GetContext(string original, string matchString)
        {
            try
            {
                int contextBytes = myConfig.Options.MatchContextBytes;
                if (contextBytes == 0)
                {
                    return "";
                }

                if (original.Length <= (contextBytes * 2))
                {
                    return original;
                }

                int foundIndex = original.IndexOf(matchString, StringComparison.OrdinalIgnoreCase);

                int contextStart = SubtractWithFloor(foundIndex, contextBytes, 0);
                string matchContext = "";
                if (contextBytes > 0) matchContext = original.Substring(contextStart, contextBytes * 2);

                return Regex.Escape(matchContext);
            }
            catch (ArgumentOutOfRangeException e)
            {
                return original;
            }
            catch (Exception e)
            {
                Mq.Trace(e.ToString());
                return "";
            }
        }

        internal string GetContext(string original, Regex matchRegex)
        {
            int contextBytes = myConfig.Options.MatchContextBytes;
            if (contextBytes == 0)
            {
                return "";
            }

            if (original.Length < 6)
            {
                return original;
            }

            int foundIndex = matchRegex.Match(original).Index;

            int contextStart = SubtractWithFloor(foundIndex, contextBytes, 0);
            string matchContext = "";
            if (contextBytes > 0) matchContext = original.Substring(contextStart, contextBytes * 2);
            return Regex.Escape(matchContext);
        }

        internal int SubtractWithFloor(int num1, int num2, int floor)
        {
            int result = num1 - num2;
            if (result <= floor) return floor;
            return result;
        }
    }

    public class TextResult
    {
        public List<string> MatchedStrings { get; set; }
        public string MatchContext { get; set; }
    }
}