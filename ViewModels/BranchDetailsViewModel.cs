using Caliburn.Micro;
using EMT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EMT.ViewModels
{
    public class BranchDetailsViewModel : Screen, IHandle<MissionFileModel>, IHandle<MissionBranchModel>
    {
        private IEventAggregator _eventAggregator;
        private MissionBranchModel _branch;

        public MissionBranchModel Branch
        {
            get { return _branch; }
            set
            {
                _branch = value;
                NotifyOfPropertyChange(() => Branch);
            }
        }
        public MissionFileModel MissionFile { get; set; }

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

        public BranchDetailsViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);
        }

        public Task HandleAsync(MissionFileModel message, CancellationToken cancellationToken)
        {
            MissionFile = message;
            return Task.CompletedTask;
        }

        public Task HandleAsync(MissionBranchModel message, CancellationToken cancellationToken)
        {
            Branch = message;
            return Task.CompletedTask;
        }
    }
}
