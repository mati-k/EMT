using EMT.Helpers;
using EMT.Models;
using System.ComponentModel;

namespace EMT.ViewModels
{
    public class BranchDetailsViewModel : ViewModelBase
    {
        public MissionBranchModel Branch { get; }

        /// <summary>
        /// Why the branch key can't be saved, null if it's fine.
        /// </summary>
        public string? NameError => ScriptNames.BranchNameProblem(Branch);

        public BranchDetailsViewModel(MissionBranchModel branch)
        {
            Branch = branch;
            Branch.PropertyChanged += Branch_PropertyChanged;
        }

        private void Branch_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MissionBranchModel.Name))
                OnPropertyChanged(nameof(NameError));
        }
    }
}
