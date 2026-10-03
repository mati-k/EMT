using EMT.Helpers.Script;
using EMT.Models;

namespace EMT.Tests
{
    public class MissionFileWriterTests
    {
        private const string Sample =
            "# File header\n" +
            "\n" +
            "branch_a = {\n" +
            "\tslot = 1\n" +
            "\tgeneric = no\n" +
            "\tai = yes\n" +
            "\tpotential = { tag = AAA }\n" +
            "\thas_country_shield = yes\n" +
            "\n" +
            "\t# comment for first\n" +
            "\tmission_one = {\n" +
            "\t\ticon = icon_one\n" +
            "\t\trequired_missions = { }\n" +
            "\t\tposition = 1\n" +
            "\t\tai_weight = { factor = 5 }\n" +
            "\t\tcompleted_by = 1466.10.19\n" +
            "\t\ttrigger = { num_of_cities > 5 } # inline\n" +
            "\t\teffect = { add_prestige = 10 }\n" +
            "\t}\n" +
            "\n" +
            "\t# comment for second\n" +
            "\tmission_two = {\n" +
            "\t\ticon = \"icon_two\"\n" +
            "\t\trequired_missions = { mission_one }\n" +
            "\t\ttrigger = { }\n" +
            "\t\teffect = { }\n" +
            "\t}\n" +
            "}\n" +
            "\n" +
            "branch_b = {\n" +
            "\tslot = 2\n" +
            "\tpotential = { tag = BBB }\n" +
            "}\n";

        private const string MissionOne =
            "\t# comment for first\n" +
            "\tmission_one = {\n" +
            "\t\ticon = icon_one\n" +
            "\t\trequired_missions = { }\n" +
            "\t\tposition = 1\n" +
            "\t\tai_weight = { factor = 5 }\n" +
            "\t\tcompleted_by = 1466.10.19\n" +
            "\t\ttrigger = { num_of_cities > 5 } # inline\n" +
            "\t\teffect = { add_prestige = 10 }\n" +
            "\t}\n";

        private const string MissionTwo =
            "\t# comment for second\n" +
            "\tmission_two = {\n" +
            "\t\ticon = \"icon_two\"\n" +
            "\t\trequired_missions = { mission_one }\n" +
            "\t\ttrigger = { }\n" +
            "\t\teffect = { }\n" +
            "\t}\n";

        private const string BranchAHeader =
            "branch_a = {\n" +
            "\tslot = 1\n" +
            "\tgeneric = no\n" +
            "\tai = yes\n" +
            "\tpotential = { tag = AAA }\n" +
            "\thas_country_shield = yes\n" +
            "\n";

        private static MissionFileModel Load(string text) =>
            MissionFileModel.Load(new TextFile() { Text = text, NewLine = "\n" });

        private static MissionModel Mission(MissionFileModel file, string name) =>
            file.Branches.SelectMany(branch => branch.Missions).First(mission => mission.Name == name);

        [Fact]
        public void NoChanges_KeepsTextExactly()
        {
            Assert.Equal(Sample, MissionFileWriter.Render(Load(Sample)));
        }

        [Fact]
        public void ReorderMissions_MovesCommentsWithMissions()
        {
            var file = Load(Sample);
            var missions = file.Branches[0].Missions;
            missions.Move(1, 0);

            string expected = "# File header\n\n" + BranchAHeader + MissionTwo + "\n" + MissionOne + "}\n\nbranch_b = {\n\tslot = 2\n\tpotential = { tag = BBB }\n}\n";
            Assert.Equal(expected, MissionFileWriter.Render(file));
        }

        [Fact]
        public void MoveMissionToBranchWithoutMissions_PutsItBeforeClosingBrace()
        {
            var file = Load(Sample);
            var mission = Mission(file, "mission_two");
            file.Branches[0].Missions.Remove(mission);
            file.Branches[1].Missions.Add(mission);

            string expected = "# File header\n\n" + BranchAHeader + MissionOne + "}\n\nbranch_b = {\n\tslot = 2\n\tpotential = { tag = BBB }\n\n" + MissionTwo + "}\n";
            Assert.Equal(expected, MissionFileWriter.Render(file));
        }

        [Fact]
        public void ReorderBranches_KeepsBranchText()
        {
            var file = Load(Sample);
            file.Branches.Move(1, 0);

            string expected = "# File header\n\nbranch_b = {\n\tslot = 2\n\tpotential = { tag = BBB }\n}\n\n" + BranchAHeader + MissionOne + "\n" + MissionTwo + "}\n";
            Assert.Equal(expected, MissionFileWriter.Render(file));
        }

        [Fact]
        public void DeleteMission_RemovesItsComment()
        {
            var file = Load(Sample);
            file.Branches[0].Missions.RemoveAt(0);

            string result = MissionFileWriter.Render(file);
            Assert.DoesNotContain("comment for first", result);
            Assert.DoesNotContain("mission_one = {", result);
            Assert.Contains(MissionTwo, result);
        }

        [Fact]
        public void EditFields_PatchesOnlyThoseValues()
        {
            var file = Load(Sample);
            var mission = Mission(file, "mission_one");
            mission.Name = "mission_renamed";
            mission.Icon = "icon_new";
            mission.Position = 3;
            mission.RequiredMissions.Add(new MissionModel() { Name = "other_mission" });
            Mission(file, "mission_two").Icon = "icon_two_new";

            string expected = Sample
                .Replace("\tmission_one = {", "\tmission_renamed = {")
                .Replace("icon = icon_one", "icon = icon_new")
                .Replace("required_missions = { }", "required_missions = { other_mission }")
                .Replace("position = 1", "position = 3")
                .Replace("\"icon_two\"", "\"icon_two_new\"");

            Assert.Equal(expected, MissionFileWriter.Render(file));
        }

        [Fact]
        public void MissingKey_InsertedAfterOpeningBrace()
        {
            var file = Load(Sample);
            Mission(file, "mission_two").Position = 2;
            file.Branches[1].Slot = 5;

            string result = MissionFileWriter.Render(file);
            Assert.Contains("\tmission_two = {\n\t\tposition = 2\n\t\ticon = \"icon_two\"\n", result);
            Assert.Contains("branch_b = {\n\tslot = 5\n", result);
        }

        [Fact]
        public void NewMissionAndBranch_AreValidAndReloadable()
        {
            var file = Load(Sample);
            file.Branches[0].Missions.Add(new MissionModel() { Name = "new_mission", Icon = "icon_x", Position = 4 });

            var branch = new MissionBranchModel(file) { Name = "branch_new", Slot = 3 };
            branch.Missions.Add(new MissionModel() { Name = "branch_new_mission", Icon = "icon_y", Position = 1 });
            branch.Missions[0].RequiredMissions.Add(new MissionModel() { Name = "mission_one" });
            file.Branches.Add(branch);

            string result = MissionFileWriter.Render(file);
            Assert.StartsWith("# File header\n\n" + BranchAHeader + MissionOne + "\n" + MissionTwo + "\n\tnew_mission = {\n", result);

            var reloaded = Load(result);
            Assert.Equal(["branch_a", "branch_b", "branch_new"], reloaded.Branches.Select(b => b.Name));
            Assert.Equal(["mission_one", "mission_two", "new_mission"], reloaded.Branches[0].Missions.Select(m => m.Name));
            Assert.Equal(3, reloaded.Branches[2].Slot);
            Assert.Equal(["mission_one"], Mission(reloaded, "branch_new_mission").RequiredMissionNames());
        }

        [Fact]
        public void EditAfterSave_UsesSavedText()
        {
            var file = Load(Sample);
            file.Branches[0].Missions.Move(1, 0);
            file.Branches[0].Missions.Add(new MissionModel() { Name = "new_mission", Icon = "icon_x" });

            string saved = MissionFileWriter.Render(file);
            file.MarkSaved(new TextFile() { Text = saved, NewLine = "\n" });
            Assert.Equal(saved, MissionFileWriter.Render(file));

            Mission(file, "new_mission").Icon = "icon_z";
            Assert.Equal(saved.Replace("icon = icon_x", "icon = icon_z"), MissionFileWriter.Render(file));
        }

        [Fact]
        public void CrLfFile_NewTextUsesCrLf()
        {
            string crlf = Sample.Replace("\n", "\r\n");
            var file = MissionFileModel.Load(new TextFile() { Text = crlf, NewLine = "\r\n" });
            Assert.Equal(crlf, MissionFileWriter.Render(file));

            Mission(file, "mission_two").Position = 2;
            file.Branches[0].Missions.Add(new MissionModel() { Name = "new_mission", Icon = "icon_x" });

            string result = MissionFileWriter.Render(file);
            Assert.DoesNotContain("\n", result.Replace("\r\n", ""));
        }
    }
}
