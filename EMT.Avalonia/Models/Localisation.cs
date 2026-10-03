using System;
using System.Collections.Generic;
using System.IO;

namespace EMT.Models
{
    public static class Localisation
    {
        public static List<Tuple<string, string>> Read(StreamReader reader)
        {
            List<Tuple<string, string>> localisation = new List<Tuple<string, string>>();

            while (!reader.EndOfStream)
            {
                string? line = reader.ReadLine();

                if (String.IsNullOrWhiteSpace(line))
                    continue;

                // Ignore comment line
                if (line.Trim().StartsWith("#"))
                    continue;

                // Lines starting without indentation are the language header
                if (line.StartsWith(" "))
                {
                    line = line.Trim();

                    string[] split = line.Split(new char[] { ' ' }, 2);
                    string[] key = split[0].Split(':');
                    string value = split[1].Substring(1, split[1].Length - 2);

                    localisation.Add(new Tuple<string, string>(key[0], value));
                }
            }

            return localisation;
        }

        public static void Write(StreamWriter writer, MissionFileModel missionFile)
        {
            writer.WriteLine("l_english:");
            foreach (MissionBranchModel branch in missionFile.Branches)
            {
                foreach (MissionModel mission in branch.Missions)
                {
                    writer.WriteLine(" {0}_title:0 \"{1}\"", mission.Name, mission.Title);
                    writer.WriteLine(" {0}_desc:0 \"{1}\"", mission.Name, mission.Description);
                    writer.WriteLine();
                }
            }
        }
    }
}
