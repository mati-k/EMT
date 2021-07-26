using AutoCompleteTextBox.Editors;
using Caliburn.Micro;
using EMT.Converters;
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
        public GroupNodeModel Root
        {
            get
            {
                if (_parent == null)
                    return (this as GroupNodeModel);

                return Parent.Root;
            }
        }
        public virtual string Name
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

        public void Select()
        {
            Providers.SuggestionProvider.Instance.SelectNode(this);
        }
    }
}
