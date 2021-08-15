using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Exceptions
{
    public class WrongPositionException : Exception
    {
        public WrongPositionException()
        {
        }

        public WrongPositionException(string message) : base(message)
        {
        }
    }
}
