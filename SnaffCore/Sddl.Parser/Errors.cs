namespace Sddl.Parser
{
    public class Error
    {
        private Error(Template T, params string[] args)
        {
            Code = T.Code;
            Description = string.Format(T.Formattable, args);
        }

        public string Code { get; }
        public string Description { get; }

        internal static Template SDP001 = new Template(nameof(SDP001), "One or many access control model (ACM) components are invalid.");
        internal static Template SDP002 = new Template(nameof(SDP002), "Unknown SID `{0}` encountered.");
        internal static Template SDP003 = new Template(nameof(SDP003), "ACE have incorrect format. {0} parts, but at least 6 are required.");
        internal static Template SDP004 = new Template(nameof(SDP004), "ACL flags part can not be fully parsed (reminder: `{0}`).");
        internal static Template SDP005 = new Template(nameof(SDP005), "ACE at positional number {0} is empty.");
        internal static Template SDP006 = new Template(nameof(SDP006), "ACL contains unexpected `{0}` characters.");
        internal static Template SDP007 = new Template(nameof(SDP007), "Unknown SDDL components encountered.");

        internal class Template
        {
            internal Template(string code, string formattable)
            {
                Code = code;
                Formattable = formattable;
            }

            public string Code { get; }
            public string Formattable { get; }

            public Error Format(params string[] args)
            {
                return new Error(this, args);
            }
        }
    }
}