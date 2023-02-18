using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EMT.Views
{
    /// <summary>
    /// Interaction logic for GfxDialogView.xaml
    /// </summary>
    public partial class GfxDialogView : Window
    {
        public GfxDialogView()
        {
            InitializeComponent();
        }

        private void FilterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                FilterTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }
    }
}
