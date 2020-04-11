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
        private Classifier classifier { get; set; }
        public TextClassifier(Classifier inClassifier)
        {
            this.classifier = inClassifier;
        }

        // TODO fix case sensitivity
        // Methods for classification
        internal bool SimpleMatch(string input)
        {
            // generic match checking
            switch (classifier.WordListType)
            {
                case MatchListType.Contains:
                    foreach (string matchString in classifier.WordList)
                    {
                        if (input.ToLower().Contains(matchString.ToLower()))
                        {
                            return true;
                        }
                    }

                    break;
                case MatchListType.EndsWith:
                    foreach (string matchString in classifier.WordList)
                    {
                        if (input.ToLower().EndsWith(matchString.ToLower()))
                        {
                            return true;
                        }
                    }

                    break;
                case MatchListType.Exact:
                    foreach (string matchString in classifier.WordList)
                    {
                        if (input.ToLower() == matchString.ToLower())
                        {
                            return true;
                        }
                    }

                    break;
                case MatchListType.StartsWith:
                    foreach (string matchString in classifier.WordList)
                    {
                        if (input.ToLower().StartsWith(matchString.ToLower()))
                        {
                            return true;
                        }
                    }

                    break;
                case MatchListType.Regex:
                    foreach (string matchString in classifier.WordList)
                    {
                        Regex regex = new Regex(matchString);
                        if (regex.IsMatch(input))
                        {
                            return true;
                        }
                    }

                    break;
                default:
                    return false;
            }

            return false;
        }

        // TODO fix up simplematch to do like this?
        internal GrepFileResult GrepFile(FileInfo fileInfo, IEnumerable<string> grepStrings, int contextBytes)
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

                    return new GrepFileResult
                    {
                        GrepContext = Regex.Escape(grepContext),
                        GreppedStrings = new List<string> { funString }
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
}