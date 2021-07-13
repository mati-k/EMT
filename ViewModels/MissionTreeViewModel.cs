using Caliburn.Micro;
using EMT.Models;
using EMT.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace EMT.ViewModels
{
    public class MissionTreeViewModel : Screen, IHandle<MissionFileModel>
    {
        private IEventAggregator _eventAggregator;
        private MissionFileModel _missionFile;
        public MissionFileModel MissionFile
        {
            get { return _missionFile; }
            set
            {
                _missionFile = value;
                NotifyOfPropertyChange(() => MissionFile);
            }
        }

        public MissionTreeViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);
        }

        public Task HandleAsync(MissionFileModel message, CancellationToken cancellationToken)
        {
            MissionFile = message;

            MissionFile.Branches.CollectionChanged += Branches_CollectionChanged;
            Branches_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, MissionFile.Branches), true);

            MissionTreeChanged();
            return Task.CompletedTask;
        }

        private void Branches_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Branches_CollectionChanged(sender, e, false);
        }

        private void Branches_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e, bool updateHandled)
        {
            if (e.NewItems != null)
            {
                foreach (MissionBranchModel branch in e.NewItems)
                {
                    branch.Missions.CollectionChanged += Missions_CollectionChanged;
                    branch.PropertyChanged += BranchPropertyChanged;
                    Missions_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, branch.Missions), true);
                }
            }

            if (e.OldItems != null)
            {
                foreach (MissionBranchModel branch in e.OldItems)
                {
                    branch.Missions.CollectionChanged -= Missions_CollectionChanged;
                    branch.PropertyChanged -= BranchPropertyChanged;
                }
            }

            if (!updateHandled)
                MissionTreeChanged();
        }

        private void Missions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Missions_CollectionChanged(sender, e, false);
        }

        private void Missions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e, bool updateHandled)
        {
            if (e.NewItems != null)
            {
                foreach (MissionModel mission in e.NewItems)
                {
                    mission.PropertyChanged += MissionPropertyChanged;
                    mission.RequiredMissions.CollectionChanged += RequiredMissions_CollectionChanged;

                    foreach (MissionModel required in mission.RequiredMissions)
                    {
                        required.PropertyChanged += MissionPropertyChanged;
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (MissionModel mission in e.OldItems)
                {
                    mission.PropertyChanged -= MissionPropertyChanged;

                    mission.RequiredMissions.CollectionChanged -= RequiredMissions_CollectionChanged;
                    foreach (MissionModel required in mission.RequiredMissions)
                    {
                        required.PropertyChanged -= MissionPropertyChanged;
                    }
                }
            }

            if (!updateHandled)
                MissionTreeChanged();
        }

        private void RequiredMissions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (MissionModel mission in e.NewItems)
                {
                    mission.PropertyChanged += MissionPropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (MissionModel mission in e.OldItems)
                {
                    mission.PropertyChanged -= MissionPropertyChanged;
                }
            }
        }

        private void BranchPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Slot" || e.PropertyName == "IsActive")
                MissionTreeChanged();
        }

        private void MissionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Position" || e.PropertyName == "Name")
                MissionTreeChanged();
        }

        private void MissionTreeChanged()
        {
            (GetView() as MissionTreeView).UpdateMissionTree();
        }

        public void SelectMission(MissionModel mission)
        {
            _eventAggregator.PublishOnUIThreadAsync(mission);
        }
    }
}