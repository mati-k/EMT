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
        private const string configPath = "cwtools-eu4-config";
        public IList<IRuleMeta> RuleStack = new List<IRuleMeta>();
        
        public List<Scope> Scopes { get; set; } = new List<Scope>();
        public List<RuleBase> TriggerRules { get; set; } = new List<RuleBase>();
        public List<RuleBase> EffectRules { get; set; } = new List<RuleBase>();

        private static ParadoxConfigParser _instance;
        public static ParadoxConfigParser Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ParadoxConfigParser();
                return _instance;
            }
        }

        public void ReadConfig()
        {
            Scopes = (Read(Path.Combine(configPath, "scopes.cwt")).Rules.First() as GroupRule).Rules.Select(rule => rule as Scope).ToList();
            Read(Path.Combine(configPath, "triggers.cwt")).Rules.ForEach(ParseRule);
            Read(Path.Combine(configPath, "effects.cwt")).Rules.ForEach(ParseRule);
        }

        public ConfigFile Read(string file)
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

            return configFile;
        }

        private Stream StreamFromArray(string[] lines)
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

        private void ParseRule(RuleBase rule)
        {
            string[] keys = rule.Name.Replace("alias[", "").Replace("]", "").Split(':');
            rule.Name = keys[1];

            if (keys[0].Equals("trigger"))
                TriggerRules.Add(rule);
            else if (keys[0].Equals("effect"))
                EffectRules.Add(rule);
        }
    }
}
