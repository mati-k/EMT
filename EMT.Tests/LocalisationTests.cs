using EMT.Helpers;
using EMT.Helpers.Script;
using EMT.Models;
using System.Text;

namespace EMT.Tests
{
    public class LocalisationTests
    {
        private const string Sample =
            "l_english:\r\n" +
            " # Missions\r\n" +
            " mission_one_title:0 \"First\"\r\n" +
            " mission_one_desc:1 \"Say \"hi\" to them\" # keep this comment\r\n" +
            " unrelated_key: \"Unrelated\"\r\n";

        private static Localisation Load(string text) =>
            new Localisation(TextFile.Read(new UTF8Encoding(true).GetPreamble().Concat(Encoding.UTF8.GetBytes(text)).ToArray(), utf8ByDefault: true));

        private static string Save(Localisation localisation)
        {
            using var stream = new MemoryStream();
            localisation.Save(stream);
            byte[] bytes = stream.ToArray();
            Assert.Equal(new UTF8Encoding(true).GetPreamble(), bytes[..3]);
            return Encoding.UTF8.GetString(bytes[3..]);
        }

        [Fact]
        public void Read_HandlesQuotesAndComments()
        {
            var localisation = Load(Sample);
            Assert.True(localisation.TryGet("mission_one_desc", out string description));
            Assert.Equal("Say \"hi\" to them", description);
            Assert.True(localisation.TryGet("unrelated_key", out string unrelated));
            Assert.Equal("Unrelated", unrelated);
        }

        [Fact]
        public void NoChanges_KeepsTextExactly()
        {
            Assert.Equal(Sample, Save(Load(Sample)));
        }

        [Fact]
        public void Set_ChangesOnlyValue()
        {
            var localisation = Load(Sample);
            localisation.Set("mission_one_desc", "New text");
            Assert.Equal(Sample.Replace("Say \"hi\" to them", "New text"), Save(localisation));
        }

        [Fact]
        public void Set_NewKeyAppendedWithSameLineEnding()
        {
            var localisation = Load(Sample);
            localisation.Set("mission_two_title", "Second");
            Assert.Equal(Sample + " mission_two_title:0 \"Second\"\r\n", Save(localisation));
        }

        [Fact]
        public void Rename_KeepsVersionAndComment()
        {
            var localisation = Load(Sample);
            localisation.Rename("mission_one_desc", "mission_renamed_desc");
            Assert.Equal(Sample.Replace("mission_one_desc:1", "mission_renamed_desc:1"), Save(localisation));
        }

        [Fact]
        public void EmptyFile_GetsHeader()
        {
            var localisation = new Localisation(TextFile.NewLocalisation());
            localisation.Set("mission_title", "Title");
            Assert.Equal("l_english:\r\n mission_title:0 \"Title\"\r\n", Save(localisation));
        }

        [Fact]
        public void UpdateLocalisation_RenamesAndAddsOnlyNeededKeys()
        {
            var localisation = Load(Sample);
            var file = MissionFileModel.Load(new TextFile() { Text = "b = {\n\tslot = 1\n\tmission_one = {\n\t\ticon = x\n\t}\n}\n", NewLine = "\n" });
            var mission = file.Branches[0].Missions[0];
            mission.Title = "First";
            mission.Description = "Say \"hi\" to them";
            mission.Name = "mission_renamed";
            file.Branches[0].Missions.Add(new MissionModel() { Name = "no_text_mission" });

            MissionFileHelper.UpdateLocalisation(localisation, file.Branches[0].Missions);

            string result = Save(localisation);
            Assert.Contains(" mission_renamed_title:0 \"First\"", result);
            Assert.Contains(" mission_renamed_desc:1 \"Say \"hi\" to them\" # keep this comment", result);
            Assert.DoesNotContain("no_text_mission", result);
        }
    }
}
