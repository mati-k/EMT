using EMT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace EMT.SharedData
{
    public class DefaultPotential : PropertyChangedBase
    {
        private NodeModel _potential;
        private static DefaultPotential _instance;

        public static DefaultPotential Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DefaultPotential();

                return _instance;
            }
        }

        public NodeModel Potential
        {
            get { return _potential; }
            set
            {
                _potential = value;
                NotifyOfPropertyChange(() => Potential);
            }
        }

        private DefaultPotential()
        {
            Potential = new GroupNodeModel() { Name = "potential" };
            (Potential as GroupNodeModel).Nodes.Add(new ValueNodeModel() { Name = "tag", Value = "AAA" , Parent = _potential as GroupNodeModel});
        }
    }
}
