using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using EMT.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EMT.ViewModels
{
    public record GfxEntry(string Name, string Path);

    public partial class GfxPickerViewModel : ViewModelBase
    {
        private readonly List<GfxEntry> _allIcons;

        [ObservableProperty]
        private string _filterText = "";

        [ObservableProperty]
        private List<GfxEntry> _filteredIcons;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
        private GfxEntry? _selectedIcon;

        public GfxPickerViewModel(string? currentIcon)
        {
            var gfxService = Ioc.Default.GetService<IGfxService>();
            _allIcons = gfxService?.MissionGfx
                .Select(gfx => new GfxEntry(gfx.Key, gfx.Value))
                .OrderBy(gfx => gfx.Name, StringComparer.OrdinalIgnoreCase)
                .ToList() ?? [];

            _filteredIcons = _allIcons;
            _selectedIcon = _allIcons.FirstOrDefault(gfx => gfx.Name == currentIcon);
        }

        partial void OnFilterTextChanged(string value)
        {
            FilteredIcons = string.IsNullOrWhiteSpace(value)
                ? _allIcons
                : _allIcons.Where(gfx => gfx.Name.Contains(value.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
        }

        [RelayCommand(CanExecute = nameof(CanConfirm))]
        public void Confirm()
        {
            DialogHost.Close(MainWindowViewModel.DialogHostId, SelectedIcon?.Name);
        }

        public bool CanConfirm()
        {
            return SelectedIcon != null;
        }

        [RelayCommand]
        public void Cancel()
        {
            DialogHost.Close(MainWindowViewModel.DialogHostId, null);
        }
    }
}
