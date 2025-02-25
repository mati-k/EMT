using Pdoxcl2Sharp;
using System;
using System.Linq;

namespace EMT.Models
{
    public class ValueNodeModel : NodeModel
    {
        private string _value;
        private bool _quoted;

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                NotifyOfPropertyChange(() => Value);
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
            if (Name.Contains("name") || Name.Contains("has_dlc") || Value.Any(character => Char.IsWhiteSpace(character)))
                valueWrite = valueWrite | ValueWrite.Quoted;

            writer.Write(Name, Value, valueWrite);
        }
    }
}
