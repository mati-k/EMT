using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Models
{
    public class GfxModel : IParadoxRead
    {
        public string Name { get; set; }
        public string TextureFile { get; set; }

        public void TokenCallback(ParadoxParser parser, string token)
        {
            if (token == "name")
                Name = parser.ReadString();

            else if (token == "texturefile")
                TextureFile = parser.ReadString();
        }
    }
}
