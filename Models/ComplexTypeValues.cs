using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Models
{
    public class ComplexTypeValues
    {
        public string Key { get; set; }
        public string Localisation { get; set; }

        public ComplexTypeValues(string key)
        {
            this.Key = key;
        }
    }
}
