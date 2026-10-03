using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using EMT.Models;
using System.Threading.Tasks;

namespace EMT.ViewModels
{
    public partial class MissionDetailsViewModel : ViewModelBase
    {
        public MissionModel Mission { get; }

        public MissionDetailsViewModel(MissionModel mission)
        {
            Mission = mission;
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
        public void AddRequiredMission()
        {
            Mission.RequiredMissions.Add(new MissionModel());
        }

        [RelayCommand]
        public void RemoveRequiredMission(MissionModel required)
        {
            Mission.RequiredMissions.Remove(required);
        }
    }
}
