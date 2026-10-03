using EMT.Exceptions;
using Pdoxcl2Sharp;
using System;
using System.Collections.ObjectModel;

namespace EMT.Models
{
    public class MissionFileModel : IParadoxRead, IParadoxWrite
    {
        public string FileName { get; set; } = "";
        public ObservableCollection<MissionBranchModel> Branches { get; } = new ObservableCollection<MissionBranchModel>();

        public void TokenCallback(ParadoxParser parser, string token)
        {
            Branches.Add(parser.Parse(new MissionBranchModel(this) { Name = token }));
        }

        public void Write(ParadoxStreamWriter writer)
        {
            foreach (MissionBranchModel branch in Branches)
            {
                if (String.IsNullOrWhiteSpace(branch.Name))
                    throw new BranchNameException();

                writer.WriteLine(branch.Name + " = {");
                branch.Write(writer);
                writer.WriteLine("}");
            }
        }
    }
}
