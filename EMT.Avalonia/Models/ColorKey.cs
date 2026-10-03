using Avalonia.Media;
using Avalonia.Media.Immutable;
using System;
using System.Collections.Generic;

namespace EMT.Models
{
    /// <summary>
    /// Text colour usable in localisation as §X text §!
    /// </summary>
    public class ColorKey
    {
        public char Key { get; }
        public IBrush Brush { get; }

        public ColorKey(char key, List<string> rgb)
        {
            this.Key = key;
            this.Brush = new ImmutableSolidColorBrush(Color.FromRgb((byte)Int32.Parse(rgb[0]), (byte)Int32.Parse(rgb[1]), (byte)Int32.Parse(rgb[2])));
        }
    }
}
