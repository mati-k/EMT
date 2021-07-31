using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace EMT.Models
{
    public class ColorKey
    {
        public char Key { get; set; }
        public Brush Brush { get; set; }

        public ColorKey(char key, List<string> rgb)
        {
            this.Key = key;
            this.Brush = new SolidColorBrush(Color.FromRgb((byte)Int32.Parse(rgb[0]), (byte)Int32.Parse(rgb[1]), (byte)Int32.Parse(rgb[2])));
        }

        public void CopyFontColor()
        {
            System.Windows.Clipboard.SetText(string.Format("§{0} §!", Key));
        }
    }
}
