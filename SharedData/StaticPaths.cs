using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace EMT.SharedData
{
    public class StaticPaths : PropertyChangedBase
    {
        private static StaticPaths _instance;
        public static StaticPaths Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new StaticPaths();
                return _instance; 
            }
        }

        private string _missionFramePath;
        public string MissionFramePath
        {
            get { return _missionFramePath; }
            set
            {
                _missionFramePath = value;
                NotifyOfPropertyChange(() => MissionFramePath);
            }
        }
    }
}
