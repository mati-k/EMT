using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CWTools.Rules;

namespace EMT.CWToolsImplementation
{
    public class ValueRuleData
    {
        public String Localisation { get; set; }
        public String Description { get; set; }
        public Options Options { get; set; }
        public NewField Field { get; set; }

        public ValueRuleData(Options options, string description)
        {
            Options = options;
        }
    }
}
