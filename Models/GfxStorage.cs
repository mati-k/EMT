using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Models
{
    public class GfxStorage
    {
        private static GfxStorage _instance;
        public static GfxStorage Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GfxStorage();
                return _instance;
            }
        }

        public Dictionary<string, string> GfxFiles { get; set; }
    }
}
