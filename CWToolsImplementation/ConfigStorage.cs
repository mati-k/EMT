using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.CWToolsImplementation
{
    public class ConfigStorage
    {
        private static ConfigStorage _instance;
        public static ConfigStorage Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ConfigStorage();
                return _instance;
            }
        }

        public SavedModData SavedData { get; set; }
        public Dictionary<string, Dictionary<string, List<string>>> ValueRules { get; set; }
        public Dictionary<string, Dictionary<string, List<string>>> NodeRules { get; set; }
    }
}
