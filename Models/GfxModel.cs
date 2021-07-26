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
            if (token.Equals("name".ToLower()))
                Name = parser.ReadString();

            else if (token.Equals("texturefile".ToLower()))
                TextureFile = parser.ReadString();
        }
    }
}
