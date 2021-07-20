using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Rules
{
    public class Scope : RuleBase
    {
        public IList<string> Aliases = new List<string>();
        public IList<string> ParentScopes = new List<string>();

        public override void TokenCallback(ParadoxParser parser, string token)
        {
            if (token.Equals("aliases"))
                Aliases = parser.ReadStringList();
            else if (token.Equals("is_subscope_of"))
                ParentScopes = parser.ReadStringList();
        }
    }
}
