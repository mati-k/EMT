using CommunityToolkit.Mvvm.ComponentModel;
using EMT.Helpers.Script;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace EMT.Models
{
    /// <summary>
    /// Branch with the fields this tool edits. Settings like potential stay as text in the file.
    /// </summary>
    public partial class MissionBranchModel : ObservableObject
    {
        /// <summary>
        /// Branch level keys that aren't missions.
        /// </summary>
        public static readonly HashSet<string> SettingKeys = ["slot", "generic", "ai", "has_country_shield", "potential", "potential_on_load"];

        [ObservableProperty]
        private string _name = "";

        [ObservableProperty]
        private int _slot = 1;

        /// <summary>
        /// Shown in the preview, not saved.
        /// </summary>
        [ObservableProperty]
        private bool _isActive = true;

        public ObservableCollection<MissionModel> Missions { get; } = new ObservableCollection<MissionModel>();

        public MissionFileModel MissionFile { get; set; }

        /// <summary>
        /// Where the branch is in the loaded file text, null for branches added in the tool.
        /// </summary>
        public ScriptNode? Source { get; private set; }

        public string? SavedName { get; private set; }
        public int SavedSlot { get; private set; }

        public MissionBranchModel(MissionFileModel missionFile)
        {
            this.MissionFile = missionFile;
            Missions.CollectionChanged += Missions_CollectionChanged;
        }

        private void Missions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Keep back-reference correct whenever a mission is moved between branches
            if (e.NewItems != null)
            {
                foreach (MissionModel mission in e.NewItems)
                    mission.Branch = this;
            }
        }

        public static bool IsMissionNode(ScriptNode node) =>
            node.IsGroup && !node.IsBare && node.Operator != null && !SettingKeys.Contains(node.Name);

        public static MissionBranchModel FromNode(ScriptNode node, MissionFileModel missionFile)
        {
            var branch = new MissionBranchModel(missionFile) { Name = node.Name };

            if (node.Child("slot") is { IsGroup: false } slot)
            {
                if (!int.TryParse(slot.Value, out int value))
                    throw new ScriptParseException($"Slot of branch '{node.Name}' should be a number, not '{slot.Value}'", slot.ValueStart);
                branch.Slot = value;
            }

            foreach (ScriptNode child in node.Children!)
            {
                if (IsMissionNode(child))
                    branch.Missions.Add(MissionModel.FromNode(child, branch));
            }

            branch.MarkSaved(node);
            return branch;
        }

        /// <summary>
        /// Called after loading or saving, with the branch's place in the current file text.
        /// </summary>
        public void MarkSaved(ScriptNode node)
        {
            Source = node;
            SavedName = Name;
            SavedSlot = Slot;
        }
    }
}
