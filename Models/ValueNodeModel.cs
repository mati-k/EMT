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
        private string _value;

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
            if (Name.Contains("name"))
                valueWrite = valueWrite | ValueWrite.Quoted;
            writer.Write(Name, Value, valueWrite);
        }
    }
}
