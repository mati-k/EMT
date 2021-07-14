using Caliburn.Micro;
using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Models
{
    public abstract class NodeModel : PropertyChangedBase, IParadoxRead, IParadoxWrite
    {
        private GroupNodeModel _parent;
        private string _name;

        public GroupNodeModel Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                NotifyOfPropertyChange(() => Parent);
            }
        }
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public abstract void TokenCallback(ParadoxParser parser, string token);
        public abstract void Write(ParadoxStreamWriter writer, ValueWrite valueWrite);
        public abstract bool SinglePath();
        public abstract NodeModel Copy();
        public virtual void Write(ParadoxStreamWriter writer)
        {
            Write(writer, ValueWrite.LeadingTabs | ValueWrite.NewLine);
        }
    }
}
