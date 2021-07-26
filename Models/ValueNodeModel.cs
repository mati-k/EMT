using EMT.Converters;
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
        public string Value { get; set; }

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
                //if (Root.Name.Equals("effect"))
                //    types = ParadoxConfigParser.Instance.EffectRules.Where(rule => rule is ValueRule).Where(rule => rule.Name.Equals(Name)).Select(rule => (rule as ValueRule).Value).ToList();
                //else
                //    types = ParadoxConfigParser.Instance.TriggerRules.Where(rule => rule is ValueRule).Where(rule => rule.Name.Equals(Name)).Select(rule => (rule as ValueRule).Value).ToList();

                if (types.Count == 0)
                    return "Key not found";

                return types.Aggregate((agg, next) => $"{agg} | {next}");
            }
        }
    }
}
