using System.Collections.Generic;

namespace Sddl.Parser
{
    internal class StringLengthComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            int result = x.Length.CompareTo(y.Length);

            if (result == 0)
            {
                return x.CompareTo(y);
            }
            else
            {
                return -result;
            }
        }
    }
}
