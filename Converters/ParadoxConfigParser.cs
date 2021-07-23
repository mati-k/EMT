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
        private List<string> foldersToCheck = new List<string>() { "events", "common" };

        public IList<IRuleMeta> RuleStack = new List<IRuleMeta>();
        
        public List<Scope> Scopes { get; set; } = new List<Scope>();
        public Dictionary<string, IList<string>> Enums { get; set; } = new Dictionary<string, IList<string>>();
        public List<RuleBase> TriggerRules { get; set; } = new List<RuleBase>();
        public List<RuleBase> EffectRules { get; set; } = new List<RuleBase>();
        // subtype = type.subtype => [type][subtype]; type = type => [type][]
        public Dictionary<string, Dictionary<string, List<ComplexTypeValues>>> TypesValues { get; set; } = new Dictionary<string, Dictionary<string, List<ComplexTypeValues>>>(); 

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
            Enums = (Read(Path.Combine(configPath, "enums.cwt")).Rules.First() as GroupRule).Rules.Select(rule => rule as ParadoxEnum)
                .ToDictionary(enumRule => enumRule.Name.Replace("enum[", "").Replace("]", ""), enumRule => enumRule.Values);


            Read(Path.Combine(configPath, "triggers.cwt")).Rules.ForEach(ParseRule);
            Read(Path.Combine(configPath, "effects.cwt")).Rules.ForEach(ParseRule);
            Read(Path.Combine(configPath, "scope_links.cwt")).Rules.ForEach(ParseRule);

            List<RuleBase> types = new List<RuleBase>();
            foreach (string folder in foldersToCheck)
            {
                foreach (string file in Directory.GetFiles(Path.Combine(configPath, folder), "*.cwt"))
                {
                    // Special file, parse seperately
                    if (!Path.GetFileName(file).Equals("links.cwt"))
                        types.AddRange((Read(file).Rules.Where(rule => rule.Name == "types").First() as GroupRule).Rules);
                }
            }

            ParseTypes(types);
        }

        public ConfigFile Read(string file)
        {
            string[] read = File.ReadAllLines(file);
            for (int i = 0; i < read.Length; i++)
            {
                if (read[i].Contains("##") && !read[i].Contains("###"))
                {
                    read[i] = String.Format("option_token = {{ {0} }}", read[i].Replace("#", ""));
                    
                    if (read[i].Contains("<>"))
                    {
                        read[i] = read[i].Replace("<>", "=");
                        read[i] = string.Format(@"{0} option_token = {{ NOT = yes }}", read[i]);
                    }
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

        private void ParseTypes(List<RuleBase> types)
        {
            string vanillaFolder = @"G:\Steam\steamapps\common\Europa Universalis IV";
            string modFolder = @"C:\Users\Mati\Documents\Paradox Interactive\Europa Universalis IV\mod\Anbennar-PublicFork";

            foreach (RuleBase rule in types)
            {
                GroupRule ruleType = rule as GroupRule;
                ruleType.Name = ruleType.Name.Replace("type[", "").Replace("]", "");

                string path = (ruleType.Rules.Where(r => r.Name.Equals("path")).First() as ValueRule).Value;
                path = path.Replace("game/", "");
                // check both base and mode, mod only if files got same name
                //path = Path.Combine(modFolder, path);

                string nameField = "";
                if (ruleType.Rules.Where(r => r.Name.Equals("name_field")).Any())
                    nameField = (ruleType.Rules.Where(r => r.Name.Equals("name_field")).First() as ValueRule).Value;

                List<string> filesToParse = new List<string>();
                try
                {
                    if (ruleType.Rules.Where(r => r.Name.Equals("path_file")).Any())
                    {
                        string pathFile = (ruleType.Rules.Where(r => r.Name.Equals("path_file")).First() as ValueRule).Value;
                        if (File.Exists(Path.Combine(modFolder, pathFile)))
                            filesToParse.Add(Path.Combine(modFolder, pathFile));
                        else
                            filesToParse.Add(Path.Combine(vanillaFolder, pathFile));
                    }
                    else
                    {
                        string[] vanilla = Directory.GetFiles(Path.Combine(vanillaFolder, path));
                        string[] mod = Directory.GetFiles(Path.Combine(modFolder, path));

                        foreach (string vanillFile in vanilla)
                        {
                            if (!mod.Select(f => Path.GetFileName(f)).Contains(Path.GetFileName(vanillFile)))
                                filesToParse.Add(vanillFile);
                        }

                        filesToParse.AddRange(mod);
                    }
                }
                catch (Exception)
                {
                    Trace.TraceError(@"Not found file of type {0}", rule.Name);
                    continue;
                }

                List<NodeModel> values = new List<NodeModel>();
                foreach (string file in filesToParse)
                {
                    try
                    {
                        GroupNodeModel groupNodeModel;
                        using (FileStream fileStream = new FileStream(file, FileMode.Open))
                        {
                            groupNodeModel = ParadoxParser.Parse(fileStream, new GroupNodeModel());
                        }

                        values.AddRange(groupNodeModel.Nodes);
                    }

                    catch (Exception)
                    {
                        Trace.TraceError(@"Not found file of type {0}", rule.Name);
                        continue;
                    }
                }

                TypesValues.Add(rule.Name, new Dictionary<string, List<ComplexTypeValues>>());
                TypesValues[rule.Name].Add("", new List<ComplexTypeValues>());

                var subTypes = ruleType.Rules.Where(r => r.Name.StartsWith("subtype") && r.Meta.Where(m => m is TypeFilter).Any()).ToList();
                foreach (var sub in subTypes) 
                {
                    sub.Name = sub.Name.Replace("subtype[", "").Replace("]", "");
                    TypesValues[rule.Name].Add(sub.Name, new List<ComplexTypeValues>());
                }

                //name_from_file
                //type_per_file
                //start_from_root

                values.RemoveAll(n => n.Name.Equals("namespace") || n.Name.Equals("normal_or_historical_nations"));
                if (ruleType.Rules.Where(m => m is ValueListRule).Any())
                {
                    var val = ruleType.Rules.Where(m => m is ValueListRule).First() as ValueListRule;
                    for (int i = 0; i < val.Values.Count; i++)
                    {
                        if (val.Values[i].Equals("any"))
                        {
                            values = values.Where(v => v is GroupNodeModel).Select(v => v as GroupNodeModel).SelectMany(v => v.Nodes).ToList();
                        }

                        else
                        {
                            values = values.Where(v => v.Name.Equals(val.Values[i])).Where(v => v is GroupNodeModel).Select(v => v as GroupNodeModel).SelectMany(v => v.Nodes).ToList();
                        }
                    }
                }

                foreach (NodeModel node in values)
                {
                    ComplexTypeValues newValue;
                    string subtypePath = "";

                    var subtypesMatches = subTypes.Where(s => s.Meta.Where(m => m is TypeFilter)
                        .Select(m => m as TypeFilter).First().Matches(node.Name));

                    if (subtypesMatches.Any())
                    {
                        subtypePath = subtypesMatches.First().Name;

                        if (!String.IsNullOrWhiteSpace(nameField))
                        {
                            newValue = new ComplexTypeValues(((node as GroupNodeModel).Nodes.Where(n => n.Name.Equals(nameField)).First() as ValueNodeModel).Value);
                        }
                        else
                        {
                            newValue = new ComplexTypeValues(node.Name);
                        }
                    }

                    else
                    {
                        if (!String.IsNullOrWhiteSpace(nameField))
                        {
                            newValue = new ComplexTypeValues(((node as GroupNodeModel).Nodes.Where(n => n.Name.Equals(nameField)).First() as ValueNodeModel).Value);
                        }
                        else
                        {
                            newValue = new ComplexTypeValues(node.Name);
                        }
                    }

                    TypesValues[rule.Name][subtypePath].Add(newValue);
                }
            }
        }
    }
}
