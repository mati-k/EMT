using EMT.Models;

namespace EMT.ViewModels
{
    public class BranchDetailsViewModel : ViewModelBase
    {
        public MissionBranchModel Branch { get; }

        public BranchDetailsViewModel(MissionBranchModel branch)
        {
            Branch = branch;
        }
    }
}
