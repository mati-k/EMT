using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Rules
{
    public class Cardinality : IRuleMeta
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public bool Strict { get; set; }

        public Cardinality(string value)
        {
            Strict = !value.StartsWith("~");
            value = value.Replace("~", "");

            string[] range = value.Split('.');


            Min = int.Parse(range[0]);

            if (range[range.Length - 1].Equals("inf"))
                Max = -1;
            else
                Max = int.Parse(range[range.Length - 1]);
        }

        public bool WithinRange(int count)
        {
            if (count < Min)
                return false;

            if (count > Max && Max != -1)
                return false;

            return true;
        }
    }
}
