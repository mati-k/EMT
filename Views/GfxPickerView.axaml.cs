using Avalonia.Controls;
using Avalonia.Input;
using EMT.ViewModels;

namespace EMT.Views
{
    public partial class GfxPickerView : UserControl
    {
        public GfxPickerView()
        {
            InitializeComponent();
        }

        private void Icons_DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (DataContext is GfxPickerViewModel viewModel && viewModel.ConfirmCommand.CanExecute(null))
                viewModel.ConfirmCommand.Execute(null);
        }
    }
}
