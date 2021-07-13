using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace EMT.Models
{
    public static class FontColors
    {
        public static Dictionary<char, Brush> TextColors { get; set; } = new Dictionary<char, Brush> {
            { 'M', new SolidColorBrush(Color.FromRgb(35, 206, 255)) },
            { 'W', new SolidColorBrush(Color.FromRgb(255, 255, 255)) },
            { 'B', new SolidColorBrush(Color.FromRgb(0, 0, 255)) },
            { 'G', new SolidColorBrush(Color.FromRgb(0, 255, 0)) },
            { 'R', new SolidColorBrush(Color.FromRgb(255, 50, 50)) },
            { 'b', new SolidColorBrush(Color.FromRgb(0, 0, 0)) },
            { 'g', new SolidColorBrush(Color.FromRgb(176, 176, 176)) },
            { 'Y', new SolidColorBrush(Color.FromRgb(255, 189, 255)) },
            { 'T', new SolidColorBrush(Color.FromRgb(0, 255, 239)) },
            { 'O', new SolidColorBrush(Color.FromRgb(255, 130, 0)) },
            { 'l', new SolidColorBrush(Color.FromRgb(154, 193, 75)) },
        };
    }
}
