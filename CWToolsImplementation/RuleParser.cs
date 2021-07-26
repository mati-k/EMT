using CWTools.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CWTools.Utilities;

namespace EMT.CWToolsImplementation
{
    public class RuleParser
    {
        // rulles IsTypeRule => contains types definitions (might use to replace parsing)
        private SavedModData savedModData;
        private List<RootRule> rules;

        public Dictionary<string, Dictionary<string, List<string>>> ValueRules { get; set; }
        public Dictionary<string, Dictionary<string, List<string>>> NodeRules { get; set; }

        public RuleParser(SavedModData savedModData, List<RootRule> rules)
        {
            this.savedModData = savedModData;
            this.rules = rules;

            ValueRules = new Dictionary<string, Dictionary<string, List<string>>>();
            ValueRules.Add("trigger", new Dictionary<string, List<string>>());
            ValueRules.Add("effect", new Dictionary<string, List<string>>());

            NodeRules = new Dictionary<string, Dictionary<string, List<string>>>();
            NodeRules.Add("trigger", new Dictionary<string, List<string>>());
            NodeRules.Add("effect", new Dictionary<string, List<string>>());
        }

        public void Parse()
        {
            foreach (RootRule rootRule in rules)
            {
                if (!rootRule.IsAliasRule)
                    continue;

                RootRule.AliasRule rule = rootRule as RootRule.AliasRule;

                if (!(rule.Item1.Equals("trigger") || rule.Item1.Equals("effect")))
                    continue;

                Options options = rule.Item2.Item2;
                string descr = options.description == Microsoft.FSharp.Core.FSharpOption<string>.None ? "" : options.description.Value;

                if (rule.Item2.Item1.IsLeafRule)
                {
                    var leaf = rule.Item2.Item1 as RuleType.LeafRule;

                    string name = "";
                    if (leaf.left.IsSpecificField)
                        name = StringResource.stringManager.GetStringForID((leaf.left as NewField.SpecificField).Item.valuec.normal);
                    else if (leaf.left.IsTypeField)
                        name = ((leaf.left as NewField.TypeField).Item as TypeType.Simple).name;

                    if (!ValueRules[rule.Item1].ContainsKey(name))
                        ValueRules[rule.Item1].Add(name, new List<string>());

                    ValueRules[rule.Item1][name].Add(descr);
                
                }

                else if (rule.Item2.Item1.IsNodeRule)
                {
                    var node = rule.Item2.Item1 as RuleType.NodeRule;

                    string name = "";
                    if (node.left.IsSpecificField)
                        name = StringResource.stringManager.GetStringForID((node.left as NewField.SpecificField).Item.valuec.normal);
                    else if (node.left.IsTypeField)
                        name = ((node.left as NewField.TypeField).Item as TypeType.Simple).name;

                    if (!NodeRules[rule.Item1].ContainsKey(name))
                        NodeRules[rule.Item1].Add(name, new List<string>());

                    NodeRules[rule.Item1][name].Add(descr);
                }
            }
        }
    }
}
//+Item1   LeafRule(SpecificField(SpecificValue CWTools.Utilities.StringTokens), ValueField(Int(-2147483648, 2147483647))) CWTools.Rules.RuleType { CWTools.Rules.RuleType.LeafRule}
//+Item1   LeafRule(SpecificField(SpecificValue CWTools.Utilities.StringTokens), VariableGetField "named_unrest")    CWTools.Rules.RuleType { CWTools.Rules.RuleType.LeafRule}
