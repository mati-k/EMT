using Caliburn.Micro;
using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace EMT.Models
{
    public class GroupNodeModel : NodeModel
    {
        public BindableCollection<NodeModel> Nodes { get; set; } = new BindableCollection<NodeModel>();

        public override void TokenCallback(ParadoxParser parser, string token)
        {
            try
            {
                if (parser.NextIsBracketed())
                {
                    Nodes.Add(parser.Parse(new GroupNodeModel() { Name = token, Parent = this }));
                }

                else
                {
                    Nodes.Add(new ValueNodeModel() { Name = token, Parent = this, Value = parser.ReadString() });
                }
            }
            catch (Exception e)
            {
                StringBuilder nodePath = new StringBuilder(token);
                GroupNodeModel parent = Parent;
                while (parent != null)
                {
                    nodePath.Append(" <= " + parent.Name);
                    parent = parent.Parent;
                }

                throw new Exception($"Token exception, token: {nodePath.ToString()} \n{e.ToString()}");
            }
        }

        public override bool SinglePath()
        {
            if (Nodes.Count == 0)
                return true;
            else if (Nodes.Count == 1)
                return Nodes[0].SinglePath();

            return false;
        }

        public override NodeModel Copy()
        {
            GroupNodeModel copy = new GroupNodeModel() { Name = this.Name };
            foreach (NodeModel node in Nodes)
            {
                copy.Nodes.Add(node.Copy());
                copy.Nodes.Last().Parent = copy;
            }

            return copy;
        }

        public override void Write(ParadoxStreamWriter writer, ValueWrite valueWrite)
        {
            if (!SinglePath())
            {
                writer.Write(Name + " = {", valueWrite);
                foreach (NodeModel node in Nodes)
                {
                    node.Write(writer);
                }
                writer.Write("}", valueWrite);
            }

            else
            {
                writer.Write(Name + " = { ", valueWrite & ~ValueWrite.NewLine);
                if (Nodes.Count == 1)
                    Nodes[0].Write(writer, ValueWrite.None);
                writer.Write(" } ", valueWrite & ~ValueWrite.LeadingTabs);
            }
        }
    }
}
