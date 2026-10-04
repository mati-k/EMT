using EMT.Helpers.Script;
using EMT.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EMT.Helpers
{
    public class MissionLoadResult
    {
        public required MissionFileModel MissionFile { get; init; }
        public required Localisation Localisation { get; init; }
    }

    public static class MissionFileHelper
    {
        /// <exception cref="UserFacingException">With a message saying which file and where the problem is</exception>
        public static MissionLoadResult Load(ConfigData config)
        {
            MissionFileModel missionFile = LoadMissionFile(config.MissionFile);
            Localisation localisation = LoadLocalisation(config.LocalisationFile);

            foreach (MissionModel mission in missionFile.Branches.SelectMany(branch => branch.Missions))
            {
                if (localisation.TryGet(mission.Name + "_title", out string title))
                    mission.Title = title;

                if (localisation.TryGet(mission.Name + "_desc", out string description))
                    mission.Description = description;
            }

            return new MissionLoadResult { MissionFile = missionFile, Localisation = localisation };
        }

        private static MissionFileModel LoadMissionFile(string path)
        {
            TextFile file;
            try
            {
                file = TextFile.ReadScript(path);
            }
            catch (Exception e)
            {
                Log.Error(e, "Reading mission file {Path}", path);
                throw new UserFacingException("Couldn't read the mission file", ErrorText.ForFile(path, e), e);
            }

            try
            {
                var missionFile = MissionFileModel.Load(file);
                missionFile.FileName = path;
                return missionFile;
            }
            catch (ScriptParseException e)
            {
                Log.Error(e, "Parsing mission file {Path}", path);
                throw new UserFacingException("Mission file has an error", ErrorText.ForScript(path, file.Text, e), e);
            }
        }

        private static Localisation LoadLocalisation(string path)
        {
            try
            {
                return Localisation.Load(path);
            }
            catch (Exception e)
            {
                Log.Error(e, "Reading localisation file {Path}", path);
                throw new UserFacingException("Couldn't read the localisation file", ErrorText.ForFile(path, e), e);
            }
        }

        /// <summary>
        /// Files that were changed (or removed) outside the tool since they were loaded or last saved.
        /// </summary>
        public static List<string> ChangedOnDisk(ConfigData config, MissionLoadResult loaded)
        {
            List<string> changed = [];

            if (!SameText(config.MissionFile, loaded.MissionFile.File.Text, TextFile.ReadScript))
                changed.Add(config.MissionFile);

            if (!SameText(config.LocalisationFile, loaded.Localisation.SavedText, TextFile.ReadLocalisation))
                changed.Add(config.LocalisationFile);

            return changed;
        }

        private static bool SameText(string path, string expected, Func<string, TextFile> read)
        {
            try
            {
                return File.Exists(path) && read(path).Text == expected;
            }
            catch (Exception e)
            {
                // Can't tell, saving will report the actual problem
                Log.Warning(e, "Checking {Path} for changes", path);
                return true;
            }
        }

        /// <summary>
        /// Saves both files: backs them up if enabled, then writes each one through a temporary file,
        /// so a failed write never leaves a half written file.
        /// </summary>
        /// <returns>Error messages, empty if everything was saved</returns>
        public static List<string> Save(ConfigData config, MissionLoadResult loaded, string? backupFolder = null)
        {
            MissionFileModel missionFile = loaded.MissionFile;

            // Build text before touching files, so problems like a missing icon leave them as they were
            string missionText;
            try
            {
                missionText = MissionFileWriter.Render(missionFile);
            }
            catch (Exception e)
            {
                Log.Error(e, "Preparing mission file");
                return [$"Nothing was saved: {e.Message}"];
            }

            if (config.UseBackups)
            {
                foreach (string path in new[] { config.MissionFile, config.LocalisationFile })
                {
                    try
                    {
                        CreateBackup(path, backupFolder ?? AppPaths.BackupFolder);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Backing up {Path}", path);
                        return [$"Nothing was saved, couldn't make a backup of\n{ErrorText.ForFile(path, e)}"];
                    }
                }
            }

            List<MissionModel> missions = missionFile.Branches.SelectMany(branch => branch.Missions).ToList();
            UpdateLocalisation(loaded.Localisation, missions);

            TextFile newFile = new TextFile() { Text = missionText, Encoding = missionFile.File.Encoding, NewLine = missionFile.File.NewLine };
            if (!SafeWrite(config.MissionFile, stream => newFile.Write(stream, missionText), out string? missionError))
            {
                // Localisation would refer to renamed missions, leave it for the next save
                return [$"Mission file wasn't saved, nothing was changed:\n{missionError}"];
            }

            missionFile.MarkSaved(newFile);

            if (!SafeWrite(config.LocalisationFile, loaded.Localisation.Save, out string? localisationError))
                return [$"Mission file was saved, but localisation wasn't:\n{localisationError}"];

            loaded.Localisation.MarkSaved();
            return [];
        }

        /// <summary>
        /// Copies file to the backups folder, named with the current time. Missing file (e.g. new one) is skipped.
        /// </summary>
        public static string? CreateBackup(string path, string backupFolder)
        {
            if (!File.Exists(path))
                return null;

            Directory.CreateDirectory(backupFolder);
            string name = $"{Path.GetFileNameWithoutExtension(path)}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
            string backup = Path.Combine(backupFolder, name + Path.GetExtension(path));

            // Several saves within a second must not replace each other's backups
            for (int i = 2; File.Exists(backup); i++)
                backup = Path.Combine(backupFolder, $"{name}_{i}{Path.GetExtension(path)}");

            File.Copy(path, backup);

            // Copy keeps read-only flag of the original, backups should stay removable
            File.SetAttributes(backup, File.GetAttributes(backup) & ~FileAttributes.ReadOnly);
            return backup;
        }

        public static void UpdateLocalisation(Localisation localisation, IEnumerable<MissionModel> missions)
        {
            foreach (MissionModel mission in missions)
            {
                string? savedName = mission.Saved?.Name;
                if (savedName != null && savedName != mission.Name)
                {
                    localisation.Rename(savedName + "_title", mission.Name + "_title");
                    localisation.Rename(savedName + "_desc", mission.Name + "_desc");
                }

                SetIfNeeded(localisation, mission.Name + "_title", mission.Title);
                SetIfNeeded(localisation, mission.Name + "_desc", mission.Description);
            }
        }

        /// <summary>
        /// Existing keys are always updated, new ones only added when there's some text.
        /// </summary>
        private static void SetIfNeeded(Localisation localisation, string key, string value)
        {
            if (localisation.TryGet(key, out _) || !string.IsNullOrEmpty(value))
                localisation.Set(key, value);
        }

        /// <summary>
        /// Writes to a temporary file next to the target, then replaces the target with it.
        /// </summary>
        public static bool SafeWrite(string path, Action<Stream> write, out string? error)
        {
            string temporary = path + ".emt-tmp";
            try
            {
                using (FileStream stream = new FileStream(temporary, FileMode.Create, FileAccess.Write))
                {
                    write(stream);
                }

                File.Move(temporary, path, overwrite: true);
                error = null;
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e, "Saving {Path}", path);
                error = ErrorText.ForFile(path, e);

                try
                {
                    File.Delete(temporary);
                }
                catch (Exception cleanup)
                {
                    Log.Warning(cleanup, "Removing {Temporary}", temporary);
                }

                return false;
            }
        }
    }

    /// <summary>
    /// Error to show to the user: short title and details saying which file and where.
    /// </summary>
    public class UserFacingException : Exception
    {
        public string Details { get; }

        public UserFacingException(string title, string details, Exception inner) : base(title, inner)
        {
            Details = details;
        }
    }
}
