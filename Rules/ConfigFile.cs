using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMT.Models;

namespace EMT.Rules
{
    public class ConfigFile : IParadoxRead
    {
        public List<IRuleMeta> RulesStack { get; set; } = new List<IRuleMeta>();
        public List<RuleBase> Rules { get; set; } = new List<RuleBase>();

        public void TokenCallback(ParadoxParser parser, string token)
        {
            if (parser.NextIsBracketed())
            {
                if (token == "option_token")
                    parser.Parse(new ReadMeta());
                else
                    Rules.Add(parser.Parse(new GroupRule() { Name = token }));
            }

            else
            {
                Rules.Add(new ValueRule() { Name = token, Value = parser.ReadString() });
            }
        }
    }
}
