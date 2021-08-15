using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Exceptions
{
    public class IconException : Exception
    {
        public IconException() { }

        public IconException(string mission) :
            base(String.Format("Mission icon unassigned at mission: {0}", mission))
        {

        }
    }
}
