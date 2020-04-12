using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace Classifiers
{
    public class TextClassifier
    {
        private ClassifierRule ClassifierRule { get; set; }
        public TextClassifier(ClassifierRule inRule)
        {
            this.ClassifierRule = inRule;
        }

        // TODO fix case sensitivity
        // Methods for classification
        internal TextResult SimpleMatch(string input)
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
                                MatchedStrings = new List<string>(){matchString}
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
                                MatchedStrings = new List<string>() {matchString}
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
                                MatchedStrings = new List<string>() { matchString }
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
                                MatchedStrings = new List<string>() { matchString }
                            };
                        }
                    }

                    break;
                case MatchListType.Regex:
                    foreach (string matchString in ClassifierRule.WordList)
                    {
                        Regex regex = new Regex(matchString);
                        Match match = regex.Match(input);
                        if (match != null)
                        {
                            int index = match.Index;
                        }

                        if (regex.IsMatch(input))
                        {
                            return new TextResult()
                            {
                                MatchedStrings = new List<string>() { matchString }
                            };
                        }
                    }

                    break;
                default:
                    return null;
            }

            return null;
        }

        // TODO fix up simplematch to do like this?
        internal TextResult GrepFile(FileInfo fileInfo, List<string> grepStrings, int contextBytes)
        {
            List<string> foundStrings = new List<string>();

            string fileContents = File.ReadAllText(fileInfo.FullName);

            foreach (string funString in grepStrings)
            {
                int foundIndex = fileContents.IndexOf(funString, StringComparison.OrdinalIgnoreCase);

                if (foundIndex >= 0)
                {
                    int contextStart = SubtractWithFloor(foundIndex, contextBytes, 0);
                    string grepContext = "";
                    if (contextBytes > 0) grepContext = fileContents.Substring(contextStart, contextBytes * 2);

                    return new TextResult
                    {
                        MatchContext = Regex.Escape(grepContext),
                        MatchedStrings = new List<string> { funString }
                    };
                }
            }
            return null;
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