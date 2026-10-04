using EMT.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EMT.Helpers
{
    /// <summary>
    /// Rules for keys written into script files (mission and branch names, required missions).
    /// </summary>
    public static partial class ScriptNames
    {
        [GeneratedRegex("^[A-Za-z0-9_-]+$")]
        private static partial Regex ValidKey();

        public const string AllowedCharacters = "letters, numbers, _ and -";

        /// <summary>
        /// Why the key can't be used, null if it's fine.
        /// </summary>
        public static string? KeyProblem(string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return "Key can't be empty";

            if (!ValidKey().IsMatch(key))
                return $"Key can only contain {AllowedCharacters}, no spaces";

            return null;
        }

        /// <summary>
        /// Why the mission's key can't be used: invalid characters or used by another mission in the file.
        /// </summary>
        public static string? MissionNameProblem(MissionModel mission)
        {
            if (KeyProblem(mission.Name) is string problem)
                return problem;

            var file = mission.Branch?.MissionFile;
            bool duplicate = file != null && file.Branches.SelectMany(branch => branch.Missions).Any(other => other != mission && other.Name == mission.Name);
            return duplicate ? "Another mission already uses this key" : null;
        }

        /// <summary>
        /// Why the branch's key can't be used: invalid characters or used by another branch in the file.
        /// </summary>
        public static string? BranchNameProblem(MissionBranchModel branch)
        {
            if (KeyProblem(branch.Name) is string problem)
                return problem;

            bool duplicate = branch.MissionFile.Branches.Any(other => other != branch && other.Name == branch.Name);
            return duplicate ? "Another branch already uses this key" : null;
        }

        /// <summary>
        /// Problems with names new or changed in the tool, as messages for the user. Empty if all is fine.
        /// Names already in the file are left alone, so untouched files can always be saved as they were.
        /// </summary>
        public static List<string> FileProblems(MissionFileModel file)
        {
            List<string> problems = [];

            foreach (MissionBranchModel branch in file.Branches)
            {
                bool branchChanged = branch.Source == null || branch.SavedName != branch.Name;
                if (branchChanged && BranchNameProblem(branch) is string branchProblem)
                    problems.Add($"Branch '{branch.Name}': {branchProblem}");

                foreach (MissionModel mission in branch.Missions)
                {
                    bool nameChanged = mission.Saved == null || mission.Saved.Name != mission.Name;
                    if (nameChanged && MissionNameProblem(mission) is string missionProblem)
                        problems.Add($"Mission '{mission.Name}' in branch '{branch.Name}': {missionProblem}");

                    List<string> requiredNames = mission.RequiredMissionNames();
                    if (mission.Saved != null && requiredNames.SequenceEqual(mission.Saved.RequiredMissions))
                        continue;

                    foreach (string required in requiredNames)
                    {
                        if (KeyProblem(required) is string requiredProblem)
                            problems.Add($"Mission '{mission.Name}' requires '{required}': {requiredProblem}");
                    }
                }
            }

            // Same mistake reported once is enough
            return problems.Distinct().ToList();
        }
    }
}
