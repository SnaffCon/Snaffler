using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Sddl.Parser
{
    internal static class Match
    {
        public static LinkedList<string> ManyByPrefix(string input, IDictionary<string, string> tokensToLabels, out string reminder)
        {
            var labels = new LinkedList<string>();

            reminder = SubstituteEmptyWithNull(input);
            while (reminder != null)
            {
                string label = Match.OneByPrefix(reminder, tokensToLabels, out reminder);

                if (label != null)
                    labels.AddLast(label);
                else
                    break;
            }

            return labels;
        }

        public static LinkedList<string> ManyByUint(uint mask, IDictionary<uint, string> tokensToLabels, out uint reminder)
        {
            var labels = new LinkedList<string>();

            reminder = mask;
            while (reminder > 0)
            {
                string label = Match.OneByUint(reminder, tokensToLabels, out reminder);

                if (label != null)
                    labels.AddLast(label);
                else
                    break;
            }

            return labels;
        }

        public static string OneByPrefix(string input, IDictionary<string, string> tokensToLabels, out string reminder)
        {
            foreach (var kv in tokensToLabels)
            {
                if (input.StartsWith(kv.Key))
                {
                    reminder = SubstituteEmptyWithNull(input.Substring(kv.Key.Length));
                    return kv.Value;
                }
            }

            reminder = input;
            return null;
        }

        public static string OneByUint(uint mask, IDictionary<uint, string> tokensToLabels, out uint reminder)
        {
            foreach (var kv in tokensToLabels)
            {
                if ((mask & kv.Key) == kv.Key)
                {
                    reminder = mask - kv.Key;
                    return kv.Value;
                }
            }

            reminder = mask;
            return null;
        }

        public static string OneByRegexOrPrefix(string input, IList<(string prefix, string regex, string value)> tokensToLabels)
        {
            foreach (var kkv in tokensToLabels)
            {
                if ((!string.IsNullOrEmpty(kkv.prefix) && input.StartsWith(kkv.prefix)) || Regex.IsMatch(input, kkv.regex))
                {
                    return kkv.value;
                }
            }

            return null;
        }

        private static string SubstituteEmptyWithNull(string input)
        {
            return input == string.Empty ? null : input;
        }
    }
}