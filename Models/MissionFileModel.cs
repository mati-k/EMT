using Caliburn.Micro;
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
            Branches.Add(parser.Parse(new MissionBranchModel() { Name = token }));
        }

        public void Write(ParadoxStreamWriter writer)
        {
            foreach (MissionBranchModel branch in Branches)
            {
                writer.WriteLine(branch.Name + " = {");
                branch.Write(writer);
                writer.WriteLine("}");
            }
        }
    }
}