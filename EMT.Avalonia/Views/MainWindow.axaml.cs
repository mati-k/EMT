using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using EMT.Helpers;
using EMT.Models;
using EMT.ViewModels;
using Serilog;
using System;
using System.IO;

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
                    Header = ColorHeader(color),
                    Command = viewModel.CopyFontColorCommand,
                    CommandParameter = color,
                });
            }

            FancyTextMenu.IsEnabled = viewModel.FontColors.Count > 0;
        }

        private async void OpenBackups_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                Directory.CreateDirectory(AppPaths.BackupFolder);
                bool opened = await Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(AppPaths.BackupFolder));
                if (!opened)
                    await Dialogs.ShowInfo("Backups folder", AppPaths.BackupFolder);
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Opening backups folder");
                await Dialogs.ShowInfo("Backups folder", AppPaths.BackupFolder);
            }
        }

        /// <summary>
        /// Sample on the game's dark background, so light colours stay readable, with the code next to it.
        /// </summary>
        private static Control ColorHeader(ColorKey color)
        {
            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                Children =
                {
                    // Swatch keeps dark colours recognisable
                    new Border
                    {
                        Background = color.Brush,
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(3),
                        Width = 16,
                        Height = 16,
                        VerticalAlignment = VerticalAlignment.Center,
                    },
                    new Border
                    {
                        Background = new SolidColorBrush(Color.Parse("#FF1E2A3A")),
                        CornerRadius = new CornerRadius(4),
                        Padding = new Thickness(10, 3),
                        Width = 96,
                        Child = new TextBlock
                        {
                            Text = "Sample text",
                            Foreground = color.Brush,
                            FontWeight = FontWeight.SemiBold,
                            HorizontalAlignment = HorizontalAlignment.Center,
                        },
                    },
                    new TextBlock
                    {
                        Text = $"§{color.Key} … §!",
                        FontFamily = new FontFamily("Consolas, Menlo, monospace"),
                        VerticalAlignment = VerticalAlignment.Center,
                    },
                },
            };
        }
    }
}
