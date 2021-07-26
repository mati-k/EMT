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
        FSharpMap<string, FSharpList<NewScope.TypeDefInfo>> Types { get; set; }
        CachedRuleMetadata Meta { get; set; }
        FSharpList<Effect> Effects { get; set; }
        FSharpList<Effect> Triggers { get; set; }

        public SavedModData(FSharpMap<string, FSharpList<NewScope.TypeDefInfo>> types, CachedRuleMetadata meta, FSharpList<Effect> effects, FSharpList<Effect> triggers)
        {
            Types = types;
            Meta = meta;
            Effects = effects;
            Triggers = triggers;
        }
    }
}
