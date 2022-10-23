namespace Sddl.Parser
{
    internal static class Format
    {
        public static string Unknown(string input)
        {
            const string UnknownString = "Unknown({0})";
            return string.Format(UnknownString, input);
        }
    }
}
