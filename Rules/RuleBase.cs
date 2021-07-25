using EMT.Converters;
using EMT.Models;
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
        public IList<IRuleMeta> Meta { get; set; } = new List<IRuleMeta>();

        public RuleBase()
        {
            if (ParadoxConfigParser.Instance.RuleStack.Count > 0)
            {
                Meta = new List<IRuleMeta>(ParadoxConfigParser.Instance.RuleStack);
                ParadoxConfigParser.Instance.RuleStack.Clear();
            }
        }

        public bool MatchesSubtype(NodeModel node)
        {
            if (Meta.Where(m => m is TypeFilter).Any())
            {
                if (!Meta.Where(m => m is TypeFilter).Select(m => m as TypeFilter).First().Matches(node.Name))
                    return false;
            }

            GroupRule groupRule = this as GroupRule;
            GroupNodeModel groupNode = node as GroupNodeModel;

            if (groupRule.Rules.Count > 0)
            {
                if (groupRule.Rules.First().Name.Equals("not_empty"))
                    return true;

                for (int i = 0; i < groupRule.Rules.Count; i++)
                {
                    var matches = groupNode.Nodes.Where(n => n.Name.Equals(groupRule.Rules[i].Name));
                    if (!(groupRule.Rules[i] as ValueRule).Value.Equals("scalar"))
                        matches = matches.Where(n => (n as ValueNodeModel).Value.Equals((groupRule.Rules[i] as ValueRule).Value));

                    if (groupRule.Rules[i].Meta.Where(m => m is Cardinality).Any())
                    {
                        Cardinality card = groupRule.Rules[i].Meta.Where(m => m is Cardinality).First() as Cardinality;
                        return card.WithinRange(matches.Count());
                    }

                    else
                    {
                        return matches.Any();
                    }
                }
            }
            //enums
            return true;
        }

        public bool MatchesType(NodeModel node)
        {
            if (Meta.Where(m => m is TypeFilter).Any())
            {
                if (!Meta.Where(m => m is TypeFilter).Select(m => m as TypeFilter).First().Matches(node.Name))
                    return false;
            }

            return true;
        }

        public abstract void TokenCallback(ParadoxParser parser, string token);
    }
}
