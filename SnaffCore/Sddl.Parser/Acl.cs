using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sddl.Parser
{
    public class Acl : Acm
    {
        public string Raw { get; }

        public string[] Flags { get; }
        public Ace[] Aces { get; }

        public Acl(string acl, SecurableObjectType type = SecurableObjectType.Unknown)
        {
            Raw = acl;

            int begin = acl.IndexOf(Ace.BeginToken);

            // Flags
            var flags = begin == -1 ? acl : acl.Substring(0, begin);
            var flagsLabels = Match.ManyByPrefix(flags, SdControlsDict, out var reminder);

            if (reminder != null)
                Report(Error.SDP004.Format(reminder));

            Flags = flagsLabels.ToArray();

            // Aces
            if (begin != -1)
            {
                LinkedList<Ace> aces = new LinkedList<Ace>();

                // brackets balance: '(' = +1, ')' = -1
                int balance = 0;
                for (int end = begin; end < acl.Length; end++)
                {
                    int length = end - begin - 1;

                    if (acl[end] == Ace.BeginToken)
                    {
                        if (balance == 0)
                            begin = end;

                        balance += 1;
                    }
                    else if (acl[end] == Ace.EndToken)
                    {
                        balance -= 1;

                        if (length < 0)
                        {
                            Report(Error.SDP005.Format(begin.ToString()));
                            continue;
                        }

                        if (balance == 0)
                            aces.AddLast(new Ace(acl.Substring(begin + 1, length), type));
                    }
                    else if (balance <= 0)
                    {
                        Report(Error.SDP006.Format(acl.Substring(begin + 1, length)));

                        balance = 0;
                    }
                }

                Aces = aces.ToArray();
            }
        }

        internal static Dictionary<string, string> SdControlsDict = new Dictionary<string, string>
        {
            { "P", "PROTECTED" },
            { "AR", "AUTO_INHERIT_REQ" },
            { "AI", "AUTO_INHERITED" },
            { "NO_ACCESS_CONTROL", "NULL_ACL" },
        };

        public override string ToString()
        {
            bool anyFlags = Flags != null && Flags.Any();
            bool anyAces = Aces != null && Aces.Any();

            StringBuilder sb = new StringBuilder();

            if (anyFlags)
                sb.AppendLineEnv($"{nameof(Flags)}: {string.Join(", ", Flags)}");

            if (anyAces)
            {
                for (int i = 0; i < Aces.Length; ++i)
                {
                    sb.AppendLineEnv($"Ace[{i:00}]");
                    sb.AppendIndentEnv(Aces[i].ToString());
                }
            }

            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is Acl acl &&
                   ((Flags is null && acl.Flags is null) || (!(Flags is null) && !(acl.Flags is null) && Flags.Except(acl.Flags).Count() == 0)) &&
                   ((Aces is null && acl.Aces is null) || (!(Aces is null) && !(acl.Aces is null) && Aces.SequenceEqual(acl.Aces)));
        }

        public static bool operator ==(Acl acl0, Acl acl1)
        {
            if (acl0 is null && acl1 is null)
                return true;
            else if (acl0 is null || acl1 is null)
                return false;
            else
                return acl0.Equals(acl1);
        }

        public static bool operator !=(Acl acl0, Acl acl1)
        {
            return !(acl0 == acl1);
        }
    }
}