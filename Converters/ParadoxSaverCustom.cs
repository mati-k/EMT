using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Converters
{
    public class ParadoxSaverCustom : ParadoxSaver
    {
        private int currentIndent = 0;

        public ParadoxSaverCustom(Stream output) : base(output) { }

        public override void Write(string key, string value, ValueWrite valuetype)
        {
            if (valuetype.HasFlag(ValueWrite.LeadingTabs))
                Write(key, ValueWrite.LeadingTabs);
            else
                Write(key, ValueWrite.None);

            Writer.Write(" = ");
            Write(value, valuetype & ~ValueWrite.LeadingTabs);
        }

        public override void Write(string value, ValueWrite type)
        {
            if (type.HasFlag(ValueWrite.LeadingTabs))
            {
                int indent = value == "}" ? currentIndent - 1 : currentIndent;
                Writer.Write(new string('\t', indent));
            }

            UpdateCurrentIndentFromIndentsIn(value);
            Writer.Write(type.HasFlag(ValueWrite.Quoted) ? '"' + value + '"' : value);

            if (type.HasFlag(ValueWrite.NewLine))
            {
                Writer.WriteLine();
            }
        }

        private void UpdateCurrentIndentFromIndentsIn(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '}')
                    currentIndent--;
                else if (str[i] == '{')
                    currentIndent++;
            }
        }
    }
}
