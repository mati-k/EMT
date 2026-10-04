using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using EMT.Helpers;
using EMT.Models;
using EMT.Services;
using System;
using System.Threading.Tasks;

namespace EMT.ViewModels
{
    public partial class ConfigurationViewModel : ViewModelBase
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ContinueCommand))]
        private string _missionFile = "";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ContinueCommand))]
        private string _localisationFile = "";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ContinueCommand))]
        [NotifyPropertyChangedFor(nameof(VanillaFolderWarning))]
        private string _vanillaFolder = "";

        /// <summary>
        /// Game folder was filled in by detection, not picked by the user.
        /// </summary>
        [ObservableProperty]
        private bool _vanillaFolderDetected;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ContinueCommand))]
        private string _modFolder = "";

        public string? VanillaFolderWarning =>
            string.IsNullOrWhiteSpace(VanillaFolder) || GamePaths.IsGameFolder(VanillaFolder)
                ? null
                : "This doesn't look like the game folder, it should contain interface and missions folders";

        [ObservableProperty]
        private bool _useBackups = true;

        private readonly Func<ConfigData, Task>? _onContinue;

        public ConfigurationViewModel()
        {
            if (Design.IsDesignMode)
                return;

            var config = Ioc.Default.GetService<IConfigService>()?.ConfigData;
            if (config != null)
            {
                MissionFile = config.MissionFile;
                LocalisationFile = config.LocalisationFile;
                VanillaFolder = config.VanillaFolder;
                ModFolder = config.ModFolder;
                UseBackups = config.UseBackups;
            }

            if (string.IsNullOrWhiteSpace(VanillaFolder) && GamePaths.FindGameFolder() is string detected)
            {
                VanillaFolder = detected;
                VanillaFolderDetected = true;
            }
        }

        partial void OnVanillaFolderChanged(string value)
        {
            VanillaFolderDetected = false;
        }

        public ConfigurationViewModel(Func<ConfigData, Task> onContinue) : this()
        {
            _onContinue = onContinue;
        }

        [RelayCommand(CanExecute = nameof(CanContinue))]
        public async Task Continue()
        {
            var config = new ConfigData
            {
                MissionFile = MissionFile,
                LocalisationFile = LocalisationFile,
                VanillaFolder = VanillaFolder,
                ModFolder = ModFolder,
                UseBackups = UseBackups,
            };

            var configService = Ioc.Default.GetService<IConfigService>()!;
            await configService.SaveConfig(config);

            if (_onContinue != null)
                await _onContinue(config);
        }

        public bool CanContinue()
        {
            return !string.IsNullOrWhiteSpace(MissionFile) && !string.IsNullOrWhiteSpace(LocalisationFile)
                && !string.IsNullOrWhiteSpace(VanillaFolder) && !string.IsNullOrWhiteSpace(ModFolder);
        }
    }
}
