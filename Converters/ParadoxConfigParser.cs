using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pdoxcl2Sharp;
using System.IO;
using System.Diagnostics;
using EMT.Models;
using EMT.Rules;

namespace EMT.Converters
{
    public class ParadoxConfigParser
    {
        public static IList<IRuleMeta> RuleStack = new List<IRuleMeta>();

        public static void Read(string file)
        {
            string[] read = File.ReadAllLines(file);
            for (int i = 0; i < read.Length; i++)
            {
                if (read[i].Contains("##") && !read[i].Contains("###"))
                {
                    read[i] = String.Format("option_token = {{ {0} }}", read[i].Replace("#", ""));
                }
            }

            ConfigFile configFile;
            using (Stream stream = StreamFromArray(read))
            {
                configFile = ParadoxParser.Parse(stream, new ConfigFile());
            }
        }

        private static Stream StreamFromArray(string[] lines)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            for (int i = 0; i < lines.Length; i++)
            {
                writer.WriteLine(lines[i]);
            }

            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
