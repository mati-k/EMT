using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Caliburn.Micro;

namespace EMT.Models
{
    public class FontColors : PropertyChangedBase
    {
        private static FontColors _instance;
        public static FontColors Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FontColors();
                return _instance;
            }
        }

        public BindableCollection<ColorKey> Colors { get; set; } = new BindableCollection<ColorKey>();
    }
}
