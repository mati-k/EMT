using EMT.Helpers.Script;
using System.Collections.ObjectModel;
using System.Linq;

namespace EMT.Models
{
    public class MissionFileModel
    {
        public string FileName { get; set; } = "";
        public ObservableCollection<MissionBranchModel> Branches { get; } = new ObservableCollection<MissionBranchModel>();

        /// <summary>
        /// Current file contents, which saving patches.
        /// </summary>
        public TextFile File { get; private set; } = TextFile.NewScript();
        public ScriptNode Root { get; private set; } = ScriptParser.Parse("");

        public static bool IsBranchNode(ScriptNode node) => node.IsGroup && !node.IsBare && node.Operator != null;

        public static MissionFileModel Load(TextFile file)
        {
            var missionFile = new MissionFileModel() { File = file, Root = ScriptParser.Parse(file.Text) };

            foreach (ScriptNode node in missionFile.Root.Children!.Where(IsBranchNode))
                missionFile.Branches.Add(MissionBranchModel.FromNode(node, missionFile));

            return missionFile;
        }

        /// <summary>
        /// After saving, points all branches and missions at their place in the new text.
        /// Saved text has them in the same order as the model.
        /// </summary>
        public void MarkSaved(TextFile file)
        {
            File = file;
            Root = ScriptParser.Parse(file.Text);

            var branchNodes = Root.Children!.Where(IsBranchNode).ToList();
            for (int i = 0; i < Branches.Count; i++)
            {
                MissionBranchModel branch = Branches[i];
                branch.MarkSaved(branchNodes[i]);

                var missionNodes = branchNodes[i].Children!.Where(MissionBranchModel.IsMissionNode).ToList();
                for (int j = 0; j < branch.Missions.Count; j++)
                    branch.Missions[j].MarkSaved(missionNodes[j]);
            }
        }
    }
}
