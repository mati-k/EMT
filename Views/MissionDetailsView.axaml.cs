using Avalonia.Controls;
using Avalonia.Input;
using EMT.ViewModels;
using System;

namespace EMT.Views
{
    public partial class MissionDetailsView : UserControl
    {
        public MissionDetailsView()
        {
            InitializeComponent();
        }

        private void RequiredInput_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is MissionDetailsViewModel viewModel)
            {
                viewModel.AddRequiredMission(RequiredInput.Text);
                RequiredInput.Text = "";
                e.Handled = true;
            }
        }

        private void RequiredInput_DropDownClosed(object? sender, EventArgs e)
        {
            // Picking a suggestion adds it right away
            if (RequiredInput.SelectedItem is string name && DataContext is MissionDetailsViewModel viewModel)
            {
                viewModel.AddRequiredMission(name);
                RequiredInput.SelectedItem = null;
                RequiredInput.Text = "";
            }
        }
    }
}
