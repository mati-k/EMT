using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMT.Helpers;
using EMT.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EMT.ViewModels
{
    public partial class EditorViewModel : ViewModelBase
    {
        public MissionLoadResult Loaded { get; }
        public MissionFileModel MissionFile => Loaded.MissionFile;
        public MissionTreeViewModel TreeViewModel { get; }

        /// <summary>
        /// Selected branch or mission.
        /// </summary>
        [ObservableProperty]
        private object? _selectedItem;

        [ObservableProperty]
        private ViewModelBase? _detailsViewModel;

        public EditorViewModel(MissionLoadResult loaded)
        {
            Loaded = loaded;
            TreeViewModel = new MissionTreeViewModel(loaded.MissionFile, mission => SelectedItem = mission);
        }

        partial void OnSelectedItemChanged(object? value)
        {
            // Selection gets temporarily cleared while items are moved around, keep details open meanwhile
            if (value == null)
                return;

            DetailsViewModel = value switch
            {
                MissionModel mission => new MissionDetailsViewModel(mission),
                MissionBranchModel branch => new BranchDetailsViewModel(branch),
                _ => null
            };
        }

        /// <summary>
        /// Name not used yet: base, base_2, base_3...
        /// </summary>
        private static string UniqueName(string baseName, IEnumerable<string> used)
        {
            HashSet<string> taken = [.. used];
            string name = baseName;
            for (int i = 2; taken.Contains(name); i++)
                name = $"{baseName}_{i}";

            return name;
        }

        [RelayCommand]
        public void AddBranch()
        {
            var branch = new MissionBranchModel(MissionFile) { Name = UniqueName("new_branch", MissionFile.Branches.Select(other => other.Name)) };
            MissionFile.Branches.Add(branch);
            SelectedItem = branch;
        }

        public void RemoveBranch(MissionBranchModel branch)
        {
            MissionFile.Branches.Remove(branch);
            if (DetailsViewModel is BranchDetailsViewModel details && details.Branch == branch
                || DetailsViewModel is MissionDetailsViewModel missionDetails && missionDetails.Mission.Branch == branch)
            {
                DetailsViewModel = null;
            }
        }

        public void AddMission(MissionBranchModel branch)
        {
            var mission = new MissionModel(branch) { Name = UniqueName("new_mission", MissionFile.Branches.SelectMany(other => other.Missions).Select(other => other.Name)) };
            branch.Missions.Add(mission);
            SelectedItem = mission;
        }

        public void RemoveMission(MissionModel mission)
        {
            mission.Branch?.Missions.Remove(mission);
            if (DetailsViewModel is MissionDetailsViewModel details && details.Mission == mission)
                DetailsViewModel = null;
        }

        /// <summary>
        /// Moves mission to target branch, so it ends up at given index.
        /// </summary>
        public void MoveMission(MissionModel mission, MissionBranchModel target, int index)
        {
            MissionBranchModel? source = mission.Branch;
            if (source == null)
                return;

            if (source == target)
            {
                int currentIndex = source.Missions.IndexOf(mission);
                if (currentIndex < index)
                    index--;

                index = Math.Clamp(index, 0, source.Missions.Count - 1);
                if (currentIndex != index)
                    source.Missions.Move(currentIndex, index);
            }

            else
            {
                source.Missions.Remove(mission);
                target.Missions.Insert(Math.Clamp(index, 0, target.Missions.Count), mission);
            }

            SelectedItem = mission;
        }

        /// <summary>
        /// Moves branch so it ends up at given index.
        /// </summary>
        public void MoveBranch(MissionBranchModel branch, int index)
        {
            var branches = MissionFile.Branches;
            int currentIndex = branches.IndexOf(branch);
            if (currentIndex < index)
                index--;

            index = Math.Clamp(index, 0, branches.Count - 1);
            if (currentIndex != index)
                branches.Move(currentIndex, index);

            SelectedItem = branch;
        }
    }
}
