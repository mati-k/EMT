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
                                string name = (typeField.Item as TypeType.Simple).name;

                                foreach (var element in ConfigStorage.Instance.SavedData.Types[name])
                                {
                                    string loca = ConfigStorage.Instance.LocalisationBindings[name].GetValueOrDefault(element.id, "");
                                    ValueNodeValueSuggestionProvider.Instance.Suggestions.Add(new ValueNodeSuggestion(element.id, loca));
                                }
                            }

                            else
                            {

                            }
                        }

                        else if (rule.Field.IsScopeField)
                        {
                            var scopeField = rule.Field as NewField.ScopeField;
                            // CWTools.Common.EU4Constants.defaultScopeInputs;
                        }

                        else if (rule.Field.IsValueField)
                        {
                            var value = rule.Field as NewField.ValueField;
                            
                            if (value.Item.IsEnum)
                            {
                                if (value.Item.enumc.Equals("country_tags"))
                                {
                                    ValueNodeValueSuggestionProvider.Instance.Suggestions.AddRange(
                                        ConfigStorage.Instance.LocalisationBindings["country_tags"].Select(l => new ValueNodeSuggestion(l.Key, l.Value)));
                                }

                                else
                                {
                                    ValueNodeValueSuggestionProvider.Instance.Suggestions.AddRange(
                                        ConfigStorage.Instance.SavedData.Meta.enumDefs[value.Item.enumc].Item2.Select(e => new ValueNodeSuggestion(e, "")));
                                }
                            }

                            else if (value.Item.IsBool)
                            {
                                ValueNodeValueSuggestionProvider.Instance.Suggestions.AddRange(new ValueNodeSuggestion[] {
                                    new ValueNodeSuggestion("no", ""),
                                    new ValueNodeSuggestion("yes", "") });
                            }

                            else
                            {

                            }
                        }

                        else
                        {
                            //ConfigStorage.Instance.SavedData.Meta.enumDefs
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
