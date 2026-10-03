using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
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
        private string _vanillaFolder = "";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ContinueCommand))]
        private string _modFolder = "";

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
            }
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
