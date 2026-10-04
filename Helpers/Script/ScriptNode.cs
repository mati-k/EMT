using System.Collections.Generic;
using System.Linq;

namespace EMT.Helpers.Script
{
    /// <summary>
    /// Entry of parsed Paradox script with its position in the source text, so it can be patched in place.
    /// Offsets are character indexes, ends are exclusive.
    /// </summary>
    public class ScriptNode
    {
        /// <summary>
        /// Key, or the item itself for bare list entries like in "required_missions = { a b }".
        /// </summary>
        public string Name { get; init; } = "";
        public int NameStart { get; init; }
        public int NameEnd { get; init; }

        /// <summary>
        /// Entry without key and operator, e.g. list item.
        /// </summary>
        public bool IsBare { get; init; }
        public string? Operator { get; init; }

        public string? Value { get; init; }
        public bool IsQuoted { get; init; }

        /// <summary>
        /// Span of the value token, including quotes.
        /// </summary>
        public int ValueStart { get; init; }
        public int ValueEnd { get; init; }

        /// <summary>
        /// Entries inside braces, null for plain values.
        /// </summary>
        public List<ScriptNode>? Children { get; init; }
        public int OpenBrace { get; init; }
        public int CloseBrace { get; set; }

        public bool IsGroup => Children != null;

        public int Start => IsBare || Operator != null ? NameStart : OpenBrace;
        public int End => IsGroup ? CloseBrace + 1 : IsBare ? NameEnd : ValueEnd;

        public ScriptNode? Child(string name) =>
            Children?.FirstOrDefault(child => !child.IsBare && child.Name == name);
    }
}
