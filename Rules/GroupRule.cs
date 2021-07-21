using EMT.Converters;
using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Rules
{
    public class GroupRule : RuleBase
    {
        public IList<RuleBase> Rules { get; set; } = new List<RuleBase>();

        public override void TokenCallback(ParadoxParser parser, string token)
        {
            if (parser.NextIsBracketed())
            {
                if (token.Equals("option_token"))
                    parser.Parse(new ReadMeta());
                else if (Name.Equals("scopes"))
                    Rules.Add(parser.Parse(new Scope() { Name = token }));
                else if (Name.Equals("enums"))
                    Rules.Add(new ParadoxEnum() { Name = token, Values = parser.ReadStringList() });
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
