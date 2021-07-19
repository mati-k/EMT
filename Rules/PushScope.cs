using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Rules
{
    public class PushScope : IRuleMeta
    {
        public string Scope { get; set; }
        
        public PushScope(String value)
        {
            Scope = value;
        }
    }
}
