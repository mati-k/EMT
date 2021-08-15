using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Exceptions
{
    public class MissionNameException : Exception
    {
        public MissionNameException()
        {
        }

        public MissionNameException(string branchName) 
            : base(string.Format("Mission without name under branch: {0}", branchName))
        {
        }
    }
}
