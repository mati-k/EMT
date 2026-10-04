using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using EMT.Helpers;
using EMT.Models;
using EMT.ViewModels;
using Serilog;
using System;
using System.IO;
using System.Linq;

namespace EMT.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AddHandler(MenuItem.SubmenuOpenedEvent, Submenu_Opened);
        }

        /// <summary>
        /// Gives dropdowns an edge, otherwise they blend into the white panels below.
        /// Theme sets the border in its template, which styles can't override, so it's set directly.
        /// </summary>
        private void Submenu_Opened(object? sender, RoutedEventArgs e)
        {
            if (e.Source is not MenuItem { Parent: Menu } menuItem)
                return;

            var popup = menuItem.GetVisualDescendants().OfType<Popup>().FirstOrDefault(child => child.Name == "PART_Popup");
            var border = (popup?.Child as Visual)?.GetVisualDescendants().OfType<Border>().FirstOrDefault(child => child.Name == "PART_MainBorder");
            if (border == null)
                return;

            border.BorderBrush = new SolidColorBrush(Color.Parse("#59000000"));
            border.BorderThickness = new Thickness(1);
            border.CornerRadius = new CornerRadius(4);
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
