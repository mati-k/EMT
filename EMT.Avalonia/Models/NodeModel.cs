using CommunityToolkit.Mvvm.ComponentModel;
using Pdoxcl2Sharp;

namespace EMT.Models
{
    public abstract partial class NodeModel : ObservableObject, IParadoxRead, IParadoxWrite
    {
        [ObservableProperty]
        private GroupNodeModel? _parent;

        [ObservableProperty]
        private string _name = "";

        public GroupNodeModel? Root
        {
            get
            {
                if (Parent == null)
                    return this as GroupNodeModel;

                return Parent.Root;
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
