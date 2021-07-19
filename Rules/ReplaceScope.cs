using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Rules
{
    public class ReplaceScope : IRuleMeta, IParadoxRead
    {
        public Dictionary<string, string> Scopes = new Dictionary<string, string>();

        public void TokenCallback(ParadoxParser parser, string token)
        {
            if (!Scopes.ContainsKey(token))
                Scopes.Add(token, parser.ReadString());
        }
    }
}
