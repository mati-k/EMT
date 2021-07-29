using EMT.CWToolsImplementation;
using EMT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CWTools.Rules;

namespace EMT.Providers
{
    public class ProviderUpdater
    {
        private static readonly ProviderUpdater _instance = new ProviderUpdater();
        public static ProviderUpdater Instance
        {
            get { return _instance; }
        }

        public void SelectNode(NodeModel node)
        {
            if (node == null || node.Name == null)
                return;

            string baseScope = node.Root.Name.Equals("effect") ? "effect" : "trigger";

            if (node is ValueNodeModel)
            {
                ValueNodeModel valueNode = node as ValueNodeModel;

                // Maybe aggregate over fields types
                ValueNodeKeySuggestionProvider.Instance.Suggestions = ConfigStorage.Instance.ValueRules[baseScope]
                    .Select(r => new ValueNodeSuggestion(r.Key, GetDescriptionValueOrEmpty(r.Value.First().Options))).ToList();

                ValueNodeValueSuggestionProvider.Instance.Suggestions.Clear();
                if (ConfigStorage.Instance.ValueRules[baseScope].ContainsKey(node.Name))
                {
                    foreach (var rule in ConfigStorage.Instance.ValueRules[baseScope][node.Name])
                    {
                        if (rule.Field.IsTypeField)
                        {
                            var typeField = rule.Field as NewField.TypeField;

                            if (typeField.Item.IsSimple)
                            {
                                ValueNodeValueSuggestionProvider.Instance.Suggestions.AddRange(ConfigStorage.Instance.LocalisationBindings[(typeField.Item as TypeType.Simple).name]
                                    .Select(t => new ValueNodeSuggestion(t.Key, t.Value)));

                            }


                        }

                        else
                        {

                        }
                    }
                }
            }
        }

        private String GetDescriptionValueOrEmpty(Options option)
        {
            if (option.description == Microsoft.FSharp.Core.FSharpOption<string>.None)
                return "";

            return option.description.Value;
        }
    }
}
