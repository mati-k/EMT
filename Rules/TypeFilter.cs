using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Rules
{
    public class TypeFilter : IRuleMeta
    {
        public List<string> Keys { get; set; }
        public bool Not { get; set; } = false;

        public TypeFilter(string key)
        {
            Keys = new List<string>();
            Keys.Add(key);
        }

        public TypeFilter(IList<string> keys)
        {
            this.Keys = new List<string>(keys);
        }

        public bool Matches(string key)
        {
            return Keys.Contains(key) ^ Not;
        }
    }
}
