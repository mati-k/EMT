using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Models
{
    public class GFXOther : IParadoxRead
    {
        public GFXOther Parent { get; set; }
        public string Name { get; set; }
        public List<GFXOther> Nodes { get; set; } = new List<GFXOther>();
        public string Value { get; set; }
        public List<string> Colors { get; set; } = new List<string>();

        public GFXOther(string name, GFXOther parent)
        {
            this.Name = name;
            this.Parent = parent;
        }

        public void TokenCallback(ParadoxParser parser, string token)
        {
            if (parser.NextIsBracketed())
            {
                if (token.Equals("color"))
                    Colors.AddRange(parser.ReadStringList());
                else if (Name.Equals("textcolors"))
                    Nodes.Add(new GFXOther(token, this) { Colors = parser.ReadStringList().ToList() });
                else
                    Nodes.Add(parser.Parse(new GFXOther(token, this)));
            }

            else
            {
                Nodes.Add(new GFXOther(token, this) { Value = parser.ReadString() });
            }
        }
    }
}
