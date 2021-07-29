using EMT.Converters;
using EMT.CWToolsImplementation;
using EMT.Providers;
using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Models
{
    public class ValueNodeModel : NodeModel
    {
        private string _value;
        private ValueNodeSuggestion _nameSuggestion;
        private ValueNodeSuggestion _valueSuggestion;

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                NotifyOfPropertyChange(() => Value);
            }
        }

        public ValueNodeSuggestion NameSuggestion
        {
            get { return _nameSuggestion; }
            set
            {
                _nameSuggestion = value;
                
                if (value != null)
                {
                    NotifyOfPropertyChange(() => NameSuggestion);
                    Name = NameSuggestion.Key;
                }
            }
        }

        public ValueNodeSuggestion ValueSuggestion
        {
            get { return _valueSuggestion; }
            set
            {
                _valueSuggestion = value;

                if (value != null)
                {
                    NotifyOfPropertyChange(() => ValueSuggestion);
                    Value = ValueSuggestion.Key;
                }
            }
        }

        public override string Name 
        { 
            get => base.Name;
            set
            {
                base.Name = value;
                NotifyOfPropertyChange(() => Watermark);
            }
        }

        public override void TokenCallback(ParadoxParser parser, string token)
        {
        }

        public override bool SinglePath()
        {
            return true;
        }

        public override NodeModel Copy()
        {
            return new ValueNodeModel() { Name = this.Name, Value = this.Value };
        }

        public override void Write(ParadoxStreamWriter writer, ValueWrite valueWrite)
        {
            if (Name.Contains("name"))
                valueWrite = valueWrite | ValueWrite.Quoted;
            writer.Write(Name, Value, valueWrite);
        }

        public string Watermark
        {
            get
            {
                List<string> types = new List<string>();

                if (Name != null && ConfigStorage.Instance.ValueRules != null)
                {
                    if (Root.Name.Equals("effect"))
                        types = ConfigStorage.Instance.ValueRules["effect"].Where(rule => rule.Key.Equals(Name)).Select(rule => rule.Value.First().Description).ToList();
                    else
                        types = ConfigStorage.Instance.ValueRules["trigger"].Where(rule => rule.Key.Equals(Name)).Select(rule => rule.Value.First().Description).ToList();
                }

                if (types.Count == 0)
                    return "Key not found";

                return types.Aggregate((agg, next) => $"{agg} | {next}");
            }
        }
    }
}
