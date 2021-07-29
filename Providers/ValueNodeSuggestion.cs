using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Providers
{
    public class ValueNodeSuggestion
    {
        public string Key { get; set; }
        public string Localisation { get; set; }

        public ValueNodeSuggestion(string key, string localisation)
        {
            this.Key = key;
            this.Localisation = localisation;
        }
    }
}
