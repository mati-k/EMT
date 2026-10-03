using Avalonia.Controls;
using EMT.Models;
using EMT.ViewModels;
using System;
using System.Collections.Specialized;

namespace EMT.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.FontColors.CollectionChanged += (_, _) => RebuildFancyTextMenu(viewModel);
                RebuildFancyTextMenu(viewModel);
            }
        }

        private void RebuildFancyTextMenu(MainWindowViewModel viewModel)
        {
            FancyTextMenu.Items.Clear();
            foreach (ColorKey color in viewModel.FontColors)
            {
                FancyTextMenu.Items.Add(new MenuItem
                {
                    Header = color.Sample,
                    Foreground = color.Brush,
                    Command = viewModel.CopyFontColorCommand,
                    CommandParameter = color,
                });
            }

            FancyTextMenu.IsEnabled = viewModel.FontColors.Count > 0;
        }
    }
}
