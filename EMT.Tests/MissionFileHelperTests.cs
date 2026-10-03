using EMT.Helpers;
using EMT.Models;

namespace EMT.Tests
{
    public class MissionFileHelperTests : IDisposable
    {
        private const string MissionText =
            "branch = {\n" +
            "\tslot = 1\n" +
            "\tmission_one = {\n" +
            "\t\ticon = icon_one\n" +
            "\t\tposition = 1\n" +
            "\t}\n" +
            "}\n";

        private const string LocalisationText = "l_english:\n mission_one_title:0 \"First\"\n";

        private readonly string _folder = Path.Combine(Path.GetTempPath(), "emt-tests-" + Guid.NewGuid().ToString("N"));
        private readonly ConfigData _config;
        private string Backups => Path.Combine(_folder, "backups");

        public MissionFileHelperTests()
        {
            Directory.CreateDirectory(_folder);
            _config = new ConfigData
            {
                MissionFile = Path.Combine(_folder, "missions.txt"),
                LocalisationFile = Path.Combine(_folder, "missions_l_english.yml"),
            };

            File.WriteAllText(_config.MissionFile, MissionText);
            File.WriteAllText(_config.LocalisationFile, LocalisationText);
        }

        public void Dispose()
        {
            Directory.Delete(_folder, recursive: true);
        }

        [Fact]
        public void Load_SyntaxError_SaysFileLineAndShowsIt()
        {
            File.WriteAllText(_config.MissionFile, "branch = {\n\tslot = 1\n\tmission_one = {\n\t\ticon = icon_one\n}\n");

            var error = Assert.Throws<UserFacingException>(() => MissionFileHelper.Load(_config));
            Assert.Contains(_config.MissionFile, error.Details);
            Assert.Contains("Line 1, column 10", error.Details);
            Assert.Contains("never closed", error.Details);
            Assert.Contains("branch = {", error.Details);
        }

        [Fact]
        public void Load_WrongValue_SaysWhere()
        {
            File.WriteAllText(_config.MissionFile, MissionText.Replace("position = 1", "position = first"));

            var error = Assert.Throws<UserFacingException>(() => MissionFileHelper.Load(_config));
            Assert.Contains("Line 5", error.Details);
            Assert.Contains("mission_one", error.Details);
            Assert.Contains("position = first", error.Details);
        }

        [Fact]
        public void Load_MissingFile_SaysWhichOne()
        {
            File.Delete(_config.LocalisationFile);

            var error = Assert.Throws<UserFacingException>(() => MissionFileHelper.Load(_config));
            Assert.Contains(_config.LocalisationFile, error.Details);
            Assert.Contains("doesn't exist", error.Details);
        }

        [Fact]
        public void Save_MakesBackupsAndWritesFiles()
        {
            var loaded = MissionFileHelper.Load(_config);
            loaded.MissionFile.Branches[0].Missions[0].Icon = "icon_new";
            loaded.MissionFile.Branches[0].Missions[0].Title = "New title";

            Assert.Empty(MissionFileHelper.Save(_config, loaded, Backups));

            Assert.Contains("icon = icon_new", File.ReadAllText(_config.MissionFile));
            Assert.Contains("\"New title\"", File.ReadAllText(_config.LocalisationFile));

            string[] backups = Directory.GetFiles(Backups);
            Assert.Equal(2, backups.Length);
            Assert.Contains(backups, backup => File.ReadAllText(backup) == MissionText);
            Assert.Contains(backups, backup => File.ReadAllText(backup) == LocalisationText);
            Assert.Empty(Directory.GetFiles(_folder, "*.emt-tmp"));
        }

        [Fact]
        public void Save_WithoutBackups_MakesNone()
        {
            var loaded = MissionFileHelper.Load(_config);
            _config.UseBackups = false;

            Assert.Empty(MissionFileHelper.Save(_config, loaded, Backups));
            Assert.False(Directory.Exists(Backups));
        }

        [Fact]
        public void Save_InvalidMission_LeavesFilesUntouched()
        {
            var loaded = MissionFileHelper.Load(_config);
            loaded.MissionFile.Branches[0].Missions.Add(new MissionModel() { Name = "no_icon" });

            var errors = MissionFileHelper.Save(_config, loaded, Backups);
            Assert.Single(errors);
            Assert.Contains("Nothing was saved", errors[0]);
            Assert.Equal(MissionText, File.ReadAllText(_config.MissionFile));
            Assert.False(Directory.Exists(Backups));
        }

        [Fact]
        public void Save_ReadOnlyFile_ReportsAndKeepsIt()
        {
            var loaded = MissionFileHelper.Load(_config);
            loaded.MissionFile.Branches[0].Missions[0].Icon = "icon_new";
            File.SetAttributes(_config.MissionFile, FileAttributes.ReadOnly);

            try
            {
                var errors = MissionFileHelper.Save(_config, loaded, Backups);
                Assert.Single(errors);
                Assert.Contains(_config.MissionFile, errors[0]);
                Assert.Equal(MissionText, File.ReadAllText(_config.MissionFile));
                Assert.Equal(LocalisationText, File.ReadAllText(_config.LocalisationFile));
                Assert.Empty(Directory.GetFiles(_folder, "*.emt-tmp"));
            }
            finally
            {
                File.SetAttributes(_config.MissionFile, FileAttributes.Normal);
            }
        }

        [Fact]
        public void ChangedOnDisk_NoticesEditsOutsideTool()
        {
            var loaded = MissionFileHelper.Load(_config);
            Assert.Empty(MissionFileHelper.ChangedOnDisk(_config, loaded));

            File.WriteAllText(_config.MissionFile, MissionText.Replace("position = 1", "position = 2"));
            Assert.Equal([_config.MissionFile], MissionFileHelper.ChangedOnDisk(_config, loaded));

            // After saving, the saved version is the new reference
            Assert.Empty(MissionFileHelper.Save(_config, loaded, Backups));
            Assert.Empty(MissionFileHelper.ChangedOnDisk(_config, loaded));
        }
    }
}
