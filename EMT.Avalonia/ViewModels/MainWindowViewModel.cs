using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using EMT.Helpers;
using EMT.Models;
using EMT.Services;
using Material.Styles.Controls;
using Material.Styles.Models;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EMT.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public const string DialogHostId = "MainDialogHost";
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
            string? modInterface = PathHelper.ResolveCaseInsensitive(config.ModFolder, "interface");
            string? vanillaInterface = PathHelper.ResolveCaseInsensitive(config.VanillaFolder, "interface");

            if (modInterface == null)
            {
                await ShowInfo("Mod folder doesn't have interface folder");
                return;
            }

            if (vanillaInterface == null)
            {
                await ShowInfo("Vanilla folder doesn't have interface folder");
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
            }
            catch (UserFacingException e)
            {
                await ShowInfo($"{e.Message}, check error log in:\n{AppPaths.LogFolder}");
            }
            catch (Exception e)
            {
                Log.Error(e, "Loading files");
                await ShowInfo($"Unexpected error when loading files, check error log in:\n{AppPaths.LogFolder}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        public async Task Save()
        {
            if (Editor == null || _config == null)
                return;

            var errors = MissionFileHelper.Save(_config, Editor.Loaded);
            if (errors.Count > 0)
            {
                await ShowInfo(string.Join("\n\n", errors) + $"\n\nCheck error log in:\n{AppPaths.LogFolder}");
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
            if (clipboard != null)
            {
                await clipboard.SetTextAsync($"§{color.Key} §!");
                Notify($"Copied §{color.Key} §! to clipboard");
            }
        }

        /// <summary>
        /// Short message at the bottom of the window.
        /// </summary>
        private static void Notify(string text)
        {
            SnackbarHost.Post(new SnackbarModel(text, TimeSpan.FromSeconds(2.5)), SnackbarHostName, DispatcherPriority.Normal);
        }

        private static async Task ShowInfo(string text)
        {
            await DialogHost.Show(new InfoDialogData(text), DialogHostId);
        }
    }
}
