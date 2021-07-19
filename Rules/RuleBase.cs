using EMT.Converters;
using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Rules
{
    public abstract class RuleBase : IParadoxRead
    {
        public string Name { get; set; }
        public IList<IRuleMeta> Meta { get; set; }

        public RuleBase()
        {
            if (ParadoxConfigParser.RuleStack.Count > 0)
            {
                Meta = new List<IRuleMeta>(ParadoxConfigParser.RuleStack);
                ParadoxConfigParser.RuleStack.Clear();
            }
        }

        public abstract void TokenCallback(ParadoxParser parser, string token);
    }
}
