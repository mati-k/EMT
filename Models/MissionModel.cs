using CommunityToolkit.Mvvm.ComponentModel;
using EMT.Helpers.Script;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EMT.Models
{
    /// <summary>
    /// Mission with the fields this tool edits. Everything else (trigger, effect...) stays as text in the file.
    /// </summary>
    public partial class MissionModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TitleOrName))]
        private string _name = "";

        [ObservableProperty]
        private int _position = 1;

        [ObservableProperty]
        private int _realPosition;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TitleOrName))]
        private string _title = "";

        [ObservableProperty]
        private string _description = "";

        [ObservableProperty]
        private string _icon = "";

        /// <summary>
        /// Required missions are kept as name-only stubs, resolved by name when needed.
        /// </summary>
        public ObservableCollection<MissionModel> RequiredMissions { get; } = new ObservableCollection<MissionModel>();

        public string TitleOrName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(Title))
                    return Name;
                return Title;
            }
        }

        /// <summary>
        /// Kept in sync by <see cref="MissionBranchModel.Missions"/>.
        /// </summary>
        public MissionBranchModel? Branch { get; set; }

        /// <summary>
        /// Where the mission is in the loaded file text, null for missions added in the tool.
        /// </summary>
        public ScriptNode? Source { get; private set; }

        /// <summary>
        /// Values as they are in the file, so only changed ones get written.
        /// </summary>
        public MissionSnapshot? Saved { get; private set; }

        public MissionModel(MissionBranchModel branch)
        {
            this.Branch = branch;
        }

        public MissionModel()
        {
        }

        public List<string> RequiredMissionNames() =>
            RequiredMissions.Select(mission => mission.Name.Trim()).Where(name => name.Length > 0).ToList();

        public static MissionModel FromNode(ScriptNode node, MissionBranchModel branch)
        {
            var mission = new MissionModel(branch) { Name = node.Name };

            if (node.Child("position") is { IsGroup: false } position)
            {
                if (!int.TryParse(position.Value, out int value))
                    throw new ScriptParseException($"Position of mission '{node.Name}' should be a number, not '{position.Value}'", position.ValueStart);
                mission.Position = value;
            }

            if (node.Child("icon") is { IsGroup: false } icon)
                mission.Icon = icon.Value ?? "";

            if (node.Child("required_missions") is { IsGroup: true } required)
            {
                foreach (ScriptNode entry in required.Children!)
                    mission.RequiredMissions.Add(new MissionModel() { Name = entry.Name });
            }

            mission.MarkSaved(node);
            return mission;
        }

        /// <summary>
        /// Called after loading or saving, with the mission's place in the current file text.
        /// </summary>
        public void MarkSaved(ScriptNode node)
        {
            Source = node;
            Saved = new MissionSnapshot(Name, Position, Icon, RequiredMissionNames());
        }

        private static bool IsRequirementLoopDFS(string startMission, HashSet<string> visited, HashSet<string> currentPath, Dictionary<string, MissionModel> missions)
        {
            // We're still on path including this mission, loop found
            if (currentPath.Contains(startMission)) { return true; }

            // Path of this mission was already checked on different path, skip
            if (visited.Contains(startMission)) { return false; }

            if (missions.ContainsKey(startMission))
            {
                currentPath.Add(startMission);
                visited.Add(startMission);

                MissionModel mission = missions[startMission];

                foreach (MissionModel requirement in mission.RequiredMissions)
                {
                    if (IsRequirementLoopDFS(requirement.Name, visited, currentPath, missions)) { return true; }
                }
            }

            currentPath.Remove(startMission);
            return false;
        }

        public static bool IsRequirementLoop(string missionName, Dictionary<string, MissionModel> missions)
        {
            return IsRequirementLoopDFS(missionName, new HashSet<string>(), new HashSet<string>(), missions);
        }
    }

    public record MissionSnapshot(string Name, int Position, string Icon, List<string> RequiredMissions);
}
