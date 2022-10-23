using Sddl.Parser;
using System;
using System.Collections.Generic;
using SnaffCore.ActiveDirectory;
using SnaffCore.Config;

namespace SnaffCore.Classifiers
{
    public class SddlAnalyser
    {

        public List<SimpleAce> AnalyseSddl(Sddl.Parser.Sddl sddl)
        {
            // simplify it once
            List<SimpleAce> simpleAC = SimplifyAC(sddl);

            return simpleAC;
        }


        public List<SimpleAce> SimplifyAC(Sddl.Parser.Sddl sddl)
        {
            List<SimpleAce> SimpleAcl = new List<SimpleAce>();
            if (sddl.Owner != null)
            {
                if (!String.IsNullOrWhiteSpace(sddl.Owner.Alias))
                {
                    SimpleAcl.Add(new SimpleAce() { ACEType = ACEType.Allow, Rights = new string[1] { "Owner" }, Trustee = new Trustee() { DisplayName = sddl.Owner.Alias } });
                }
            }
            if (sddl.Dacl != null)
            {
                if (sddl.Dacl.Aces.Length > 0)
                {
                    foreach (Ace ace in sddl.Dacl.Aces)
                    {
                        SimpleAce simpleAce = new SimpleAce
                        {
                            Trustee = new Trustee()
                            {
                                DisplayName = ace.AceSid.Alias,
                                Sid = ace.AceSid.Raw
                            }
                        };
                        switch (ace.AceType)
                        {
                            case "OBJECT_ACCESS_ALLOWED":
                                simpleAce.ACEType = ACEType.Allow;
                                break;
                            case "OBJECT_ACCESS_DENIED":
                                simpleAce.ACEType = ACEType.Deny;
                                break;
                            default:
                                break;
                        }
                        simpleAce.Rights = SimplifyRights(ace.Rights);

                        SimpleAcl.Add(simpleAce);
                    }
                }
            }
            return SimpleAcl;
        }

        public string[] SimplifyRights(string[] rights)
        {
            //TODO actually simplify these? does it matter?
            return rights;
        }
    }

    public class SimpleAce
    {
        public Trustee Trustee { get; set; }
        public ACEType ACEType { get; set; }
        //public List<SimpleRight> Rights { get; set; }
        public string[] Rights { get; set; }
    }

    public enum ACEType
    {
        Allow,
        Deny
    }
}
