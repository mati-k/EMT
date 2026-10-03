using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using EMT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EMT.ViewModels
{
    public partial class MissionDetailsViewModel : ViewModelBase
    {
        public MissionModel Mission { get; }

        /// <summary>
        /// Other missions in the file, suggested when adding requirements.
        /// </summary>
        public List<string> OtherMissionNames { get; }

        [ObservableProperty]
        private string _requiredMissionInput = "";

        public MissionDetailsViewModel(MissionModel mission)
        {
            Mission = mission;
            OtherMissionNames = mission.Branch?.MissionFile.Branches
                .SelectMany(branch => branch.Missions)
                .Where(other => other != mission)
                .Select(other => other.Name)
                .Distinct()
                .Order(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? [];
        }

        [RelayCommand]
        public async Task PickGfx()
        {
            var picker = new GfxPickerViewModel(Mission.Icon);
            var result = await DialogHost.Show(picker, MainWindowViewModel.DialogHostId);

            if (result is string icon && !string.IsNullOrWhiteSpace(icon))
                Mission.Icon = icon;
        }

        [RelayCommand]
        public void AddRequiredMission(string? name)
        {
            name = (name ?? RequiredMissionInput).Trim();
            RequiredMissionInput = "";

            if (name.Length == 0 || name == Mission.Name || Mission.RequiredMissions.Any(required => required.Name == name))
                return;

            Mission.RequiredMissions.Add(new MissionModel() { Name = name });
        }

        [RelayCommand]
        public void RemoveRequiredMission(MissionModel required)
        {
            Mission.RequiredMissions.Remove(required);
        }
    }
}
