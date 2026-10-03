using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using EMT.Helpers;
using EMT.Models;
using EMT.Services;
using Material.Styles.Controls;
using Material.Styles.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EMT.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public const string DialogHostId = Dialogs.HostId;
        public const string SnackbarHostName = "Root";

        [ObservableProperty]
        private ViewModelBase _currentPage;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private EditorViewModel? _editor;

        [ObservableProperty]
        private bool _isBusy;

        public ObservableCollection<ColorKey> FontColors { get; } = new();

        private ConfigData? _config;

        public MainWindowViewModel()
        {
            _currentPage = new ConfigurationViewModel(MoveToEditor);
        }

        private async Task MoveToEditor(ConfigData config)
        {
            // Mod doesn't need its own interface folder, it may use only vanilla icons
            if (!GamePaths.IsGameFolder(config.VanillaFolder))
            {
                await Dialogs.ShowError("Game folder doesn't look right",
                    $"{config.VanillaFolder}\nIt should contain interface and missions folders, pick the Europa Universalis IV installation folder.");
                return;
            }

            if (!Directory.Exists(config.ModFolder))
            {
                await Dialogs.ShowError("Mod folder doesn't exist", $"{config.ModFolder}\nIt may have been moved or deleted, pick it again.");
                return;
            }

            IsBusy = true;
            try
            {
                var gfxService = Ioc.Default.GetService<IGfxService>()!;
                var result = await Task.Run(() =>
                {
                    gfxService.Load(config.VanillaFolder, config.ModFolder);
                    return MissionFileHelper.Load(config);
                });

                FontColors.Clear();
                foreach (ColorKey color in gfxService.TextColors)
                    FontColors.Add(color);

                _config = config;
                Editor = new EditorViewModel(result);
                CurrentPage = Editor;

                if (gfxService.LoadWarnings.Count > 0)
                    await ShowGfxWarnings(gfxService.LoadWarnings);
            }
            catch (UserFacingException e)
            {
                await Dialogs.ShowError(e.Message, e.Details);
            }
            catch (Exception e)
            {
                Log.Error(e, "Loading files");
                await Dialogs.ShowError("Unexpected error when loading files", e.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static async Task ShowGfxWarnings(IReadOnlyList<string> warnings)
        {
            const int shown = 3;
            string details = string.Join("\n\n", warnings.Take(shown));
            if (warnings.Count > shown)
                details += $"\n\n...and {warnings.Count - shown} more";

            await Dialogs.ShowError($"{warnings.Count} interface file(s) couldn't be read, icons defined in them won't be shown", details);
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        public async Task Save()
        {
            if (Editor == null || _config == null)
                return;

            try
            {
                List<string> changed = MissionFileHelper.ChangedOnDisk(_config, Editor.Loaded);
                if (changed.Count > 0)
                {
                    bool overwrite = await Dialogs.Confirm("Files changed outside the tool",
                        "These files were changed (for example in a text editor) since they were loaded:\n\n" + string.Join("\n", changed)
                        + "\n\nSaving will replace them with the version in the tool, those changes will be lost"
                        + (_config.UseBackups ? " (a backup is made first)." : "."),
                        "Save anyway");

                    if (!overwrite)
                        return;
                }

                var errors = MissionFileHelper.Save(_config, Editor.Loaded);
                if (errors.Count > 0)
                    await Dialogs.ShowError("Saving failed", string.Join("\n\n", errors));
                else
                    Notify("Saved");
            }
            catch (Exception e)
            {
                Log.Error(e, "Saving");
                await Dialogs.ShowError("Unexpected error when saving", e.Message);
            }
        }

        public bool CanSave()
        {
            return Editor != null;
        }

        [RelayCommand]
        public async Task CopyFontColor(ColorKey color)
        {
            var clipboard = Ioc.Default.GetService<IClipboardService>();
            if (clipboard == null)
                return;

            try
            {
                await clipboard.SetTextAsync($"§{color.Key} §!");
                Notify($"Copied §{color.Key} §! to clipboard");
            }
            catch (Exception e)
            {
                Log.Error(e, "Copying to clipboard");
                Notify("Couldn't access the clipboard");
            }
        }

        /// <summary>
        /// Short message at the bottom of the window.
        /// </summary>
        public static void Notify(string text)
        {
            SnackbarHost.Post(new SnackbarModel(text, TimeSpan.FromSeconds(2.5)), SnackbarHostName, DispatcherPriority.Normal);
        }
    }
}
