using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;

namespace EMT.ViewModels
{
    public class GfxDialogViewModel : Screen
    {
        private Dictionary<string, string> _gfxFiles;
        public Dictionary<string, string> GfxFiles { 
            get { return _gfxFiles; }
            set
            {
                _gfxFiles = value;
                FilterGfxFiles(FilterText);
                NotifyOfPropertyChange(() => FilteredGfxFiles);
            }
        }

        public Dictionary<string, string> FilteredGfxFiles { get; set; }

        public KeyValuePair<string, string> SelectedIcon { get; set; }

        private string _filterText = "";
        public string FilterText
        {
            get { return _filterText; }
            set
            {
                _filterText = value;
                FilterGfxFiles(value);
                NotifyOfPropertyChange(() => FilterText);
                NotifyOfPropertyChange(() => FilteredGfxFiles);
            }
        }

        public void DoubleClick(MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && e.ChangedButton == MouseButton.Left)
            {
                Ok(SelectedIcon);
            }
        }

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

        public void FilterGfxFiles(string filter)
        {
            if (String.IsNullOrEmpty(filter.Trim()))
                FilteredGfxFiles = GfxFiles;

            FilteredGfxFiles = GfxFiles.Where(keyValuePair => keyValuePair.Key.Contains(filter)).ToDictionary(keyValuePair => keyValuePair.Key, keyValuePair => keyValuePair.Value);
        }
    }
}
