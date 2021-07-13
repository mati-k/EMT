using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace EMT.ViewModels
{
    public class GfxDialogViewModel : Screen
    {
        public Dictionary<string, string> GfxFiles { get; set; }
        public KeyValuePair<string, string> SelectedIcon { get; set; }

        public bool CanOk(KeyValuePair<string, string> selectedIcon)
        {
            return String.IsNullOrWhiteSpace(selectedIcon.Key);
        }

        public void Ok(KeyValuePair<string, string> selectedIcon)
        {
            TryCloseAsync(true);
        }

        public void Cancel()
        {
            TryCloseAsync(false);
        }
    }
}
