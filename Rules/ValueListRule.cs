using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Rules
{
    public class ValueListRule : RuleBase
    {
        public IList<string> Values { get; set; } = new List<string>();

        public override void TokenCallback(ParadoxParser parser, string token)
        {
        }
    }
}
