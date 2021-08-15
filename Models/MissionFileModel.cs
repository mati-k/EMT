using Caliburn.Micro;
using EMT.Exceptions;
using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.Models
{
    public class MissionFileModel : PropertyChangedBase, IParadoxRead, IParadoxWrite
    {
        public string FileName { get; set; }
        public BindableCollection<MissionBranchModel> Branches { get; set; } = new BindableCollection<MissionBranchModel>();

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