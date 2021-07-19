using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Rules
{
    public class Scope : IRuleMeta, IParadoxRead
    {
        public IList<string> Scopes = new List<string>();

        public void TokenCallback(ParadoxParser parser, string token)
        {
        }
    }
}
