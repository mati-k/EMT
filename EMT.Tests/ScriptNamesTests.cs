using EMT.Exceptions;
using EMT.Helpers;
using EMT.Helpers.Script;
using EMT.Models;

namespace EMT.Tests
{
    public class ScriptNamesTests
    {
        private const string Sample =
            "branch_a = {\n" +
            "\tslot = 1\n" +
            "\tmission_one = {\n" +
            "\t\ticon = icon_one\n" +
            "\t}\n" +
            "\tmission_two = {\n" +
            "\t\ticon = icon_two\n" +
            "\t}\n" +
            "}\n";

        private static MissionFileModel Load(string text) =>
            MissionFileModel.Load(new TextFile() { Text = text, NewLine = "\n" });

        [Theory]
        [InlineData("mission_one")]
        [InlineData("Z43-star-ascendancy")]
        [InlineData("A1_b2-C3")]
        public void KeyProblem_AllowsLettersNumbersUnderscoreAndDash(string key)
        {
            Assert.Null(ScriptNames.KeyProblem(key));
        }

        [Theory]
        [InlineData("my mission")]
        [InlineData("mission{")]
        [InlineData("a=b")]
        [InlineData("quoted\"")]
        [InlineData("comment#")]
        [InlineData("dot.ted")]
        [InlineData("")]
        [InlineData("  ")]
        public void KeyProblem_RejectsOtherCharacters(string key)
        {
            Assert.NotNull(ScriptNames.KeyProblem(key));
        }

        [Fact]
        public void MissionNameProblem_ReportsDuplicate()
        {
            var file = Load(Sample);
            file.Branches[0].Missions[1].Name = "mission_one";

            Assert.Contains("already", ScriptNames.MissionNameProblem(file.Branches[0].Missions[1]));
            Assert.Contains("already", ScriptNames.MissionNameProblem(file.Branches[0].Missions[0]));
        }

        [Fact]
        public void Render_RenamedWithSpace_NothingWritten()
        {
            var file = Load(Sample);
            file.Branches[0].Missions[0].Name = "my mission";

            var error = Assert.Throws<InvalidNameException>(() => MissionFileWriter.Render(file));
            Assert.Contains("my mission", error.Message);
        }

        [Fact]
        public void Render_InvalidBranchOrRequiredName_NothingWritten()
        {
            var file = Load(Sample);
            file.Branches[0].Name = "branch {";
            Assert.Throws<InvalidNameException>(() => MissionFileWriter.Render(file));

            file = Load(Sample);
            file.Branches[0].Missions[1].RequiredMissions.Add(new MissionModel() { Name = "a b" });
            Assert.Throws<InvalidNameException>(() => MissionFileWriter.Render(file));
        }

        [Fact]
        public void Render_NewMissionsWithSameKey_NothingWritten()
        {
            var file = Load(Sample);
            file.Branches[0].Missions.Add(new MissionModel() { Name = "new_mission", Icon = "x" });
            file.Branches[0].Missions.Add(new MissionModel() { Name = "new_mission", Icon = "x" });

            var error = Assert.Throws<InvalidNameException>(() => MissionFileWriter.Render(file));
            Assert.Contains("already", error.Message);
        }

        [Fact]
        public void Render_ValidRenames_Saved()
        {
            var file = Load(Sample);
            file.Branches[0].Name = "branch-renamed_1";
            file.Branches[0].Missions[0].Name = "Z43_star-ascendancy";

            string result = MissionFileWriter.Render(file);
            Assert.Contains("branch-renamed_1 = {", result);
            Assert.Contains("\tZ43_star-ascendancy = {", result);
        }

        [Fact]
        public void Render_ExistingProblemsUntouched_StillSaves()
        {
            // Names already in the file aren't judged, so the file can be saved as it was
            string text = Sample.Replace("mission_two", "mission_one");
            var file = Load(text);

            Assert.Equal(text, MissionFileWriter.Render(file));
        }
    }
}
