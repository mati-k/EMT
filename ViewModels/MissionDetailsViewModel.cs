using Caliburn.Micro;
using EMT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EMT.SharedData;

namespace EMT.ViewModels
{
    public class MissionDetailsViewModel : Screen, IHandle<MissionFileModel>, IHandle<MissionModel>
    {
        private IEventAggregator _eventAggregator;
        private IWindowManager _windowManager;

        private MissionModel _mission;

        public MissionModel Mission 
        {
            get { return _mission; }
            set
            {
                _mission = value;
                NotifyOfPropertyChange(() => Mission);
            }
        }

        public MissionFileModel MissionFile { get; set; }

        public MissionDetailsViewModel(IEventAggregator eventAggregator, IWindowManager windowManager)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);

            _windowManager = windowManager;
        }

        public void AddValue(GroupNodeModel node)
        {
            node.Nodes.Add(new ValueNodeModel() { Parent = node }); ;
        }

        public void AddGroup(GroupNodeModel node)
        {
            node.Nodes.Add(new GroupNodeModel() { Parent = node });
        }

        public void RemoveValue(ValueNodeModel node)
        {
            node.Parent.Nodes.Remove(node);
        }

        public void RemoveGroup(GroupNodeModel node)
        {
            node.Parent.Nodes.Remove(node);
        }

        public Task HandleAsync(MissionFileModel message, CancellationToken cancellationToken)
        {
            MissionFile = message;
            return Task.CompletedTask;
        }

        public Task HandleAsync(MissionModel message, CancellationToken cancellationToken)
        {
            Mission = message;
            return Task.CompletedTask;
        }

        public void PickGfx()
        {
            GfxDialogViewModel gfxDialog = IoC.Get<GfxDialogViewModel>();
            gfxDialog.GfxFiles = GfxStorage.Instance.GfxFiles;

            var result = _windowManager.ShowDialogAsync(gfxDialog);
            if (result.Result.GetValueOrDefault())
                Mission.Icon = gfxDialog.SelectedIcon.Key;
        }
    }
}
