using CWTools.Common;
using CWTools.Games;
using Microsoft.FSharp.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.CWToolsImplementation
{
    [Serializable]
    public class SavedModData
    {
        public Dictionary<string, FSharpList<NewScope.TypeDefInfo>> Types { get; set; }
        public CachedRuleMetadata Meta { get; set; }
        public List<Effect> Effects { get; set; }
        public List<Effect> Triggers { get; set; }

        public SavedModData(FSharpMap<string, FSharpList<NewScope.TypeDefInfo>> types, CachedRuleMetadata meta, FSharpList<Effect> effects, FSharpList<Effect> triggers)
        {
            Types = types.ToDictionary(t => t.Key, t => t.Value);
            Meta = meta;
            Effects = ListModule.ToSeq(effects).ToList();
            Triggers = ListModule.ToSeq(triggers).ToList();
        }

        public SavedModData(Dictionary<string, FSharpList<NewScope.TypeDefInfo>> types, CachedRuleMetadata meta, List<Effect> effects, List<Effect> triggers)
        {
            Types = types;
            Meta = meta;
            Effects = effects;
            Triggers = triggers;
        }
    }
}
