using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Models
{
    public class GfxFileModel : IParadoxRead
    {
        public IList<GfxModel> Gfx { get; set; }

        public GfxFileModel()
        {
            Gfx = new List<GfxModel>();
        }

        public void TokenCallback(ParadoxParser parser, string token)
        {
            if (token == null)
                return;

            if (token.Equals("spriteType"))
                Gfx.Add(parser.Parse(new GfxModel()));
        }
    }
}
