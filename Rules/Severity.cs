using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Rules
{
    public class Severity : IRuleMeta
    {
        public string Value { get; set; }

        public Severity(string value)
        {
            Value = value;
        }
    }
}
