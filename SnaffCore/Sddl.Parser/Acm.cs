using System.Collections.Generic;
using System.Linq;

namespace Sddl.Parser
{
    public abstract class Acm
    {
        public bool IsValid => Errors.Any() == false;
        public List<Error> Errors { get; } = new List<Error>();

        protected void Report(Error error) => Errors.Add(error);
    }
}