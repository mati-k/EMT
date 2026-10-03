using Avalonia.Threading;
using EMT.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace EMT.ViewModels
{
    /// <summary>
    /// Watches the mission file for anything affecting the tree preview and recalculates real mission positions.
    /// </summary>
    public class MissionTreeViewModel : ViewModelBase
    {
        private readonly List<INotifyPropertyChanged> _watchedObjects = [];
        private readonly List<INotifyCollectionChanged> _watchedCollections = [];
        private readonly Action<MissionModel> _onMissionSelected;
        private bool _updatePending;

        public MissionFileModel MissionFile { get; }

        /// <summary>
        /// Raised (at most once per UI frame) after positions were recalculated and the preview should be redrawn.
        /// </summary>
        public event Action? TreeChanged;

        public MissionTreeViewModel(MissionFileModel missionFile, Action<MissionModel> onMissionSelected)
        {
            MissionFile = missionFile;
            _onMissionSelected = onMissionSelected;

            Resubscribe();
            UpdateMissionPositions();
        }

        public void SelectMission(MissionModel mission)
        {
            _onMissionSelected(mission);
        }

        private void Resubscribe()
        {
            foreach (var watched in _watchedObjects)
                watched.PropertyChanged -= Watched_PropertyChanged;
            foreach (var collection in _watchedCollections)
                collection.CollectionChanged -= Watched_CollectionChanged;

            _watchedObjects.Clear();
            _watchedCollections.Clear();

            WatchCollection(MissionFile.Branches);
            foreach (MissionBranchModel branch in MissionFile.Branches)
            {
                Watch(branch);
                WatchCollection(branch.Missions);

                foreach (MissionModel mission in branch.Missions)
                {
                    Watch(mission);
                    WatchCollection(mission.RequiredMissions);

                    foreach (MissionModel required in mission.RequiredMissions)
                        Watch(required);
                }
            }
        }

        private void Watch(INotifyPropertyChanged watched)
        {
            watched.PropertyChanged += Watched_PropertyChanged;
            _watchedObjects.Add(watched);
        }

        private void WatchCollection(INotifyCollectionChanged collection)
        {
            collection.CollectionChanged += Watched_CollectionChanged;
            _watchedCollections.Add(collection);
        }

        private void Watched_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Resubscribe();
            ScheduleUpdate();
        }

        private void Watched_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            bool relevant = sender switch
            {
                MissionBranchModel => e.PropertyName is nameof(MissionBranchModel.Slot) or nameof(MissionBranchModel.IsActive),
                MissionModel => e.PropertyName is nameof(MissionModel.Position) or nameof(MissionModel.Name) or nameof(MissionModel.Icon),
                _ => false
            };

            if (relevant)
                ScheduleUpdate();
        }

        private void ScheduleUpdate()
        {
            if (_updatePending)
                return;

            _updatePending = true;
            Dispatcher.UIThread.Post(() =>
            {
                _updatePending = false;
                UpdateMissionPositions();
                TreeChanged?.Invoke();
            });
        }

        private void UpdateMissionPositions()
        {
            Dictionary<string, MissionModel> missions = new Dictionary<string, MissionModel>();
            foreach (MissionModel mission in MissionFile.Branches.Where(b => b.IsActive).SelectMany(branch => branch.Missions))
            {
                missions.TryAdd(mission.Name, mission);
            }

            HashSet<string> calculated = new HashSet<string>();
            foreach (MissionModel mission in missions.Values)
            {
                if (!calculated.Contains(mission.Name))
                    RecalculateRealPosition(mission, missions, calculated);
            }
        }

        private static void RecalculateRealPosition(MissionModel mission, Dictionary<string, MissionModel> missions, HashSet<string> calculated)
        {
            int max = mission.Position;
            foreach (MissionModel required in mission.RequiredMissions)
            {
                if (missions.ContainsKey(required.Name) && !MissionModel.IsRequirementLoop(required.Name, missions))
                {
                    if (!calculated.Contains(required.Name))
                        RecalculateRealPosition(missions[required.Name], missions, calculated);

                    max = Math.Max(max, missions[required.Name].RealPosition + 1);
                }
            }

            mission.RealPosition = max;
            calculated.Add(mission.Name);
        }
    }
}
