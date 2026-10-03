using EMT.Helpers.Script;
using EMT.Models;

namespace EMT.Tests
{
    /// <summary>
    /// Checks against the real game files. Set EMT_EU4_PATH to the game folder, otherwise the default Steam location is tried.
    /// </summary>
    public class GameFilesTests
    {
        private static string GamePath()
        {
            string path = Environment.GetEnvironmentVariable("EMT_EU4_PATH") ?? @"G:\Steam\steamapps\common\Europa Universalis IV";
            if (!Directory.Exists(path))
                Assert.Skip($"EU4 not found at {path}, set EMT_EU4_PATH");

            return path;
        }

        [Fact]
        public void MissionFiles_NoChanges_SavedExactly()
        {
            var files = Directory.GetFiles(Path.Combine(GamePath(), "missions"), "*.txt");
            Assert.NotEmpty(files);

            foreach (string path in files)
            {
                TextFile file = TextFile.ReadScript(path);
                var missionFile = MissionFileModel.Load(file);

                using var stream = new MemoryStream();
                file.Write(stream, MissionFileWriter.Render(missionFile));
                Assert.True(File.ReadAllBytes(path).SequenceEqual(stream.ToArray()), $"{Path.GetFileName(path)} changed");
            }
        }

        [Fact]
        public void MissionFiles_ReverseEverything_KeepsAllContent()
        {
            foreach (string path in Directory.GetFiles(Path.Combine(GamePath(), "missions"), "*.txt"))
            {
                var missionFile = MissionFileModel.Load(TextFile.ReadScript(path));
                var expected = Signatures(missionFile.Root, missionFile.File.Text);

                var branches = missionFile.Branches.Reverse().ToList();
                missionFile.Branches.Clear();
                foreach (var branch in branches)
                {
                    var missions = branch.Missions.Reverse().ToList();
                    branch.Missions.Clear();
                    foreach (var mission in missions)
                        branch.Missions.Add(mission);
                    missionFile.Branches.Add(branch);
                }

                string result = MissionFileWriter.Render(missionFile);
                var reloaded = MissionFileModel.Load(new TextFile() { Text = result });

                Assert.Equal(missionFile.Branches.Select(b => b.Name), reloaded.Branches.Select(b => b.Name));
                Assert.Equal(missionFile.Branches.SelectMany(b => b.Missions).Select(m => m.Name), reloaded.Branches.SelectMany(b => b.Missions).Select(m => m.Name));

                // Every branch and mission keeps exactly the same content, only in different order
                var actual = Signatures(reloaded.Root, result);
                Assert.Equal(expected.Order(), actual.Order());
            }
        }

        /// <summary>
        /// Text of every branch setting and every mission block, without whitespace.
        /// </summary>
        private static List<string> Signatures(ScriptNode root, string text)
        {
            List<string> result = [];
            foreach (ScriptNode branch in root.Children!.Where(MissionFileModel.IsBranchNode))
            {
                foreach (ScriptNode child in branch.Children!)
                {
                    string content = new string(text[child.Start..child.End].Where(c => !char.IsWhiteSpace(c)).ToArray());
                    result.Add(MissionBranchModel.IsMissionNode(child) ? content : branch.Name + ":" + content);
                }
            }

            return result;
        }

        [Fact]
        public void LocalisationFiles_NoChanges_SavedExactly()
        {
            var files = Directory.GetFiles(Path.Combine(GamePath(), "localisation"), "*_l_english.yml");
            Assert.NotEmpty(files);

            foreach (string path in files)
            {
                var localisation = Localisation.Load(path);
                using var stream = new MemoryStream();
                localisation.Save(stream);
                Assert.True(File.ReadAllBytes(path).SequenceEqual(stream.ToArray()), $"{Path.GetFileName(path)} changed");
            }
        }

        [Fact]
        public void GfxFiles_AllParse()
        {
            var files = Directory.GetFiles(Path.Combine(GamePath(), "interface"), "*.gfx", SearchOption.AllDirectories);
            List<string> failures = [];

            foreach (string path in files)
            {
                try
                {
                    ScriptParser.Parse(TextFile.ReadScript(path).Text);
                }
                catch (ScriptParseException e)
                {
                    failures.Add($"{Path.GetFileName(path)}: {e.Message}");
                }
            }

            Assert.Empty(failures);
        }
    }
}
