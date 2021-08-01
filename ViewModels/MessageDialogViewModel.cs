using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.ViewModels
{
    public class MessageDialogViewModel : Screen
    {
        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                NotifyOfPropertyChange(() => Message);
            }
        }

        public void Ok(KeyValuePair<string, string> selectedIcon)
        {
            TryCloseAsync(true);
        }
    }
}
