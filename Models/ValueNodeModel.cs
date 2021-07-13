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

        public override void TokenCallback(ParadoxParser parser, string token)
        {
        }

        public override bool SinglePath()
        {
            return true;
        }

        public override void Write(ParadoxStreamWriter writer, ValueWrite valueWrite)
        {
            if (Name.Contains("name"))
                valueWrite = valueWrite | ValueWrite.Quoted;
            writer.Write(Name, Value, valueWrite);
        }
    }
}
