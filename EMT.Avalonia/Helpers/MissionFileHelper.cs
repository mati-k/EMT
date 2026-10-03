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
        /// <exception cref="UserFacingException">With a message suitable to show to the user</exception>
        public static MissionLoadResult Load(ConfigData config)
        {
            MissionFileModel missionFile;
            try
            {
                missionFile = MissionFileModel.Load(TextFile.ReadScript(config.MissionFile));
                missionFile.FileName = config.MissionFile;
            }
            catch (Exception e)
            {
                Log.Error(e, "Loading mission file");
                throw new UserFacingException($"Error loading mission file: {e.Message}", e);
            }

            Localisation localisation;
            try
            {
                localisation = Localisation.Load(config.LocalisationFile);
                foreach (MissionModel mission in missionFile.Branches.SelectMany(branch => branch.Missions))
                {
                    if (localisation.TryGet(mission.Name + "_title", out string title))
                        mission.Title = title;

                    if (localisation.TryGet(mission.Name + "_desc", out string description))
                        mission.Description = description;
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Loading localisation file");
                throw new UserFacingException($"Error loading localisation file: {e.Message}", e);
            }

            return new MissionLoadResult { MissionFile = missionFile, Localisation = localisation };
        }

        /// <summary>
        /// Saves both files. Each file is backed up first and restored if writing fails.
        /// </summary>
        /// <returns>Error messages, empty if everything was saved</returns>
        public static List<string> Save(ConfigData config, MissionLoadResult loaded)
        {
            List<string> errors = new List<string>();
            MissionFileModel missionFile = loaded.MissionFile;

            // Build text before touching files, so validation errors leave them as they were
            string missionText;
            try
            {
                missionText = MissionFileWriter.Render(missionFile);
            }
            catch (Exception e)
            {
                Log.Error(e, "Preparing mission file");
                errors.Add($"Nothing saved: {e.Message}");
                return errors;
            }

            List<MissionModel> missions = missionFile.Branches.SelectMany(branch => branch.Missions).ToList();
            UpdateLocalisation(loaded.Localisation, missions);

            TextFile newFile = new TextFile() { Text = missionText, Encoding = missionFile.File.Encoding, NewLine = missionFile.File.NewLine };
            bool missionSaved = SafeWrite(config.MissionFile, "mission", errors, stream => newFile.Write(stream, missionText));
            SafeWrite(config.LocalisationFile, "localisation", errors, loaded.Localisation.Save);

            if (missionSaved)
                missionFile.MarkSaved(newFile);

            return errors;
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

        private static bool SafeWrite(string filePath, string fileKind, List<string> errors, Action<Stream> write)
        {
            string backupName = filePath;
            while (File.Exists(backupName))
                backupName = backupName + "_copy";

            bool hasBackup = File.Exists(filePath);
            if (hasBackup)
                File.Copy(filePath, backupName);

            bool success = true;
            try
            {
                using FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                write(stream);
            }
            catch (Exception e)
            {
                success = false;
                Log.Error(e, "Saving {FileKind} file", fileKind);
                errors.Add($"Error when saving {fileKind} file: {e.Message}");

                if (hasBackup)
                    File.Copy(backupName, filePath, true);
            }

            if (hasBackup)
                File.Delete(backupName);

            return success;
        }
    }

    public class UserFacingException : Exception
    {
        public UserFacingException(string message, Exception inner) : base(message, inner) { }
    }
}
