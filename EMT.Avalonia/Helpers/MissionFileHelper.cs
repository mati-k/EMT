using EMT.Models;
using Pdoxcl2Sharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EMT.Helpers
{
    public class MissionLoadResult
    {
        public required MissionFileModel MissionFile { get; init; }

        /// <summary>
        /// Localisation entries not belonging to any mission, written back unchanged on save.
        /// </summary>
        public required Dictionary<string, string> UnconnectedLocalisation { get; init; }
    }

    public static class MissionFileHelper
    {
        /// <exception cref="UserFacingException">With a message suitable to show to the user</exception>
        public static MissionLoadResult Load(ConfigData config)
        {
            MissionFileModel missionFile;
            try
            {
                using (FileStream fileStream = File.OpenRead(config.MissionFile))
                {
                    missionFile = ParadoxParser.Parse(fileStream, new MissionFileModel());
                    missionFile.FileName = config.MissionFile;
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Loading mission file");
                throw new UserFacingException("Error loading mission file", e);
            }

            Dictionary<string, string> unconnected = new Dictionary<string, string>();
            try
            {
                Dictionary<string, string> localisation = new Dictionary<string, string>();
                using (StreamReader reader = new StreamReader(File.OpenRead(config.LocalisationFile)))
                {
                    foreach (var tuple in Localisation.Read(reader))
                    {
                        if (!localisation.TryAdd(tuple.Item1, tuple.Item2))
                            Log.Warning("Duplicate localisation: {Key} = {Value}", tuple.Item1, tuple.Item2);
                    }
                }

                HashSet<string> used = new HashSet<string>();
                foreach (MissionBranchModel branch in missionFile.Branches)
                {
                    foreach (MissionModel mission in branch.Missions)
                    {
                        if (localisation.TryGetValue(mission.Name + "_title", out string? title))
                        {
                            mission.Title = title;
                            used.Add(mission.Name + "_title");
                        }

                        if (localisation.TryGetValue(mission.Name + "_desc", out string? description))
                        {
                            mission.Description = description;
                            used.Add(mission.Name + "_desc");
                        }
                    }
                }

                foreach (var entry in localisation)
                {
                    if (!used.Contains(entry.Key))
                        unconnected.Add(entry.Key, entry.Value);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Loading localisation file");
                throw new UserFacingException("Error loading localisation file", e);
            }

            return new MissionLoadResult { MissionFile = missionFile, UnconnectedLocalisation = unconnected };
        }

        /// <summary>
        /// Saves both files. Each file is backed up first and restored if writing fails.
        /// </summary>
        /// <returns>Error messages, empty if everything was saved</returns>
        public static List<string> Save(ConfigData config, MissionFileModel missionFile, Dictionary<string, string> unconnectedLocalisation)
        {
            List<string> errors = new List<string>();

            SafeWrite(config.MissionFile, "mission", errors, stream =>
            {
                using ParadoxStreamWriter writer = new ParadoxSaverCustom(stream);
                missionFile.Write(writer);
            });

            SafeWrite(config.LocalisationFile, "localisation", errors, stream =>
            {
                using StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(true));
                Localisation.Write(writer, missionFile);
                writer.WriteLine();
                foreach (var entry in unconnectedLocalisation)
                {
                    writer.WriteLine(" {0}:0 \"{1}\"", entry.Key, entry.Value);
                }
            });

            return errors;
        }

        private static void SafeWrite(string filePath, string fileKind, List<string> errors, Action<Stream> write)
        {
            string backupName = filePath;
            while (File.Exists(backupName))
                backupName = backupName + "_copy";

            bool hasBackup = File.Exists(filePath);
            if (hasBackup)
                File.Copy(filePath, backupName);

            try
            {
                using FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                write(stream);
            }
            catch (Exception e)
            {
                Log.Error(e, "Saving {FileKind} file", fileKind);
                errors.Add($"Error when saving {fileKind} file: {e.Message}");

                if (hasBackup)
                    File.Copy(backupName, filePath, true);
            }

            if (hasBackup)
                File.Delete(backupName);
        }
    }

    public class UserFacingException : Exception
    {
        public UserFacingException(string message, Exception inner) : base(message, inner) { }
    }
}
