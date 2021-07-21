using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Rules
{
    public class ParadoxEnum : RuleBase
    {
        public IList<string> Values;

        public override void TokenCallback(ParadoxParser parser, string token)
        {
        }
    }
}
