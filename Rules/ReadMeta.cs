using EMT.Converters;
using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Rules
{
    public class ReadMeta : IParadoxRead
    {
        public void TokenCallback(ParadoxParser parser, string token)
        {
            switch (token)
            {
                case "cardinality": ParadoxConfigParser.Instance.RuleStack.Add(new Cardinality(parser.ReadString())); return;
                case "push_scope": ParadoxConfigParser.Instance.RuleStack.Add(new PushScope(parser.ReadString())); return;
                case "replace_scope": ParadoxConfigParser.Instance.RuleStack.Add(parser.Parse(new ReplaceScope())); return;
                case "severity": ParadoxConfigParser.Instance.RuleStack.Add(new Severity(parser.ReadString())); return;
                case "scope":
                    ScopeMeta scope = new ScopeMeta();
                    if (parser.NextIsBracketed())
                        scope.Scopes = parser.ReadStringList();
                    else
                        scope.Scopes.Add(parser.ReadString());
                    ParadoxConfigParser.Instance.RuleStack.Add(scope);
                    return;
            }
        }
    }
}
