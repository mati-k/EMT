using CommunityToolkit.Mvvm.ComponentModel;
using EMT.Exceptions;
using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EMT.Models
{
    public partial class MissionModel : ObservableObject, IParadoxRead, IParadoxWrite
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

        [ObservableProperty]
        private GroupNodeModel? _provincesToHighlight = new GroupNodeModel() { Name = "provinces_to_highlight" };

        [ObservableProperty]
        private GroupNodeModel _trigger = new GroupNodeModel() { Name = "trigger" };

        [ObservableProperty]
        private GroupNodeModel _effect = new GroupNodeModel() { Name = "effect" };

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

        public MissionModel(MissionBranchModel branch)
        {
            this.Branch = branch;
        }

        public MissionModel()
        {
        }

        public void TokenCallback(ParadoxParser parser, string token)
        {
            try
            {
                switch (token)
                {
                    case "position": Position = parser.ReadInt32(); break;
                    case "icon": Icon = parser.ReadString(); break;
                    case "required_missions":
                        RequiredMissions.Clear();
                        foreach (string name in parser.ReadStringList())
                            RequiredMissions.Add(new MissionModel() { Name = name });
                        break;
                    case "provinces_to_highlight": ProvincesToHighlight = parser.Parse(new GroupNodeModel() { Name = "provinces_to_highlight" }); break;
                    case "trigger": Trigger = parser.Parse(new GroupNodeModel() { Name = "trigger" }); break;
                    case "effect": Effect = parser.Parse(new GroupNodeModel() { Name = "effect" }); break;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Mission exception, mission: {Name}  , token: {token} \n{e}");
            }
        }

        public void Write(ParadoxStreamWriter writer)
        {
            if (String.IsNullOrWhiteSpace(Icon))
                throw new IconException(Name);
            writer.WriteLine("icon", Icon, ValueWrite.LeadingTabs);

            if (Position <= 0)
                throw new WrongPositionException(string.Format("Position must be greater than 0, mission: {0}", Name));
            writer.WriteLine("position", Position.ToString(), ValueWrite.LeadingTabs);

            List<MissionModel> requiredMissions = RequiredMissions.Where(mission => !String.IsNullOrWhiteSpace(mission.Name)).ToList();
            if (requiredMissions.Count > 0)
            {
                if (requiredMissions.Count > 1)
                {
                    writer.WriteLine("required_missions = {", ValueWrite.LeadingTabs);
                    foreach (MissionModel required in requiredMissions)
                    {
                        writer.WriteLine(required.Name, ValueWrite.LeadingTabs);
                    }
                    writer.WriteLine("}", ValueWrite.LeadingTabs);
                }

                else
                {
                    writer.WriteLine("required_missions = { " + requiredMissions[0].Name + " } ", ValueWrite.LeadingTabs);
                }
            }

            if (ProvincesToHighlight != null)
            {
                ProvincesToHighlight.Write(writer);
                writer.WriteLine();
            }

            Trigger.Write(writer);
            writer.WriteLine();

            Effect.Write(writer);
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
}
