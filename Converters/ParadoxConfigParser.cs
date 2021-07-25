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
        private List<string> foldersToCheck = new List<string>() { "events", "common", "map" };

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


                if (read[i].Contains("scalar") && !read[i].Contains("="))
                {
                    read[i] = @"not_empty = yes";
                }

                if (read[i].Contains('!') && !read[i].Contains("!="))
                {
                    read[i] = read[i].Replace("!", "not_");
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
            Dictionary<string, string> localisations = ReadAllLocalisation();

            string vanillaFolder = @"G:\Steam\steamapps\common\Europa Universalis IV";
            string modFolder = @"C:\Users\Mati\Documents\Paradox Interactive\Europa Universalis IV\mod\Anbennar-PublicFork";

            List<string> ignoreFiles = new List<string>() { "type[wargoal_type]", "type[new_diplomatic_action]" };
            types.RemoveAll(r => ignoreFiles.Contains(r.Name));

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
                        if (File.Exists(Path.Combine(modFolder, path, pathFile)))
                            filesToParse.Add(Path.Combine(modFolder, path, pathFile));
                        else
                            filesToParse.Add(Path.Combine(vanillaFolder, path, pathFile));
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

                        if ((rule as GroupRule).Rules.Where(r => r.Name.Equals("name_from_file")).Any())
                            groupNodeModel.Name = Path.GetFileNameWithoutExtension(file);

                        if ((rule as GroupRule).Rules.Where(r => r.Name.Equals("type_per_file")).Any())
                            values.Add(groupNodeModel);
                        else
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

                var subTypes = ruleType.Rules.Where(r => r.Name.StartsWith("subtype")).ToList();
                foreach (var sub in subTypes) 
                {
                    sub.Name = sub.Name.Replace("subtype[", "").Replace("]", "");
                    TypesValues[rule.Name].Add(sub.Name, new List<ComplexTypeValues>());
                }

                values.RemoveAll(n => n.Name.Equals("namespace") || n.Name.Equals("normal_or_historical_nations"));
                if (ruleType.Rules.Where(m => m is ValueListRule).Any())
                {
                    var val = ruleType.Rules.Where(m => m is ValueListRule).First() as ValueListRule;
                    for (int i = 0; i < val.Values.Count; i++)
                    {
                        if (val.Values[i].Equals("any"))
                        {
                            values = values.Where(v => v is GroupNodeModel).Select(v => v as GroupNodeModel).SelectMany(v => v.Nodes.Where(n => n is GroupNodeModel)).ToList();
                        }

                        else
                        {
                            values = values.Where(v => v.Name.Equals(val.Values[i])).Where(v => v is GroupNodeModel).Select(v => v as GroupNodeModel).SelectMany(v => v.Nodes.Where(n => n is GroupNodeModel)).ToList();
                        }
                    }
                }

                Dictionary<string, string> locs = new Dictionary<string, string>();
                if (ruleType.Rules.Where(r => r.Name.Equals("localisation")).Any())
                {
                    var locMeta = ruleType.Rules.Where(r => r.Name.Equals("localisation")).Select(r => r as GroupRule).First();
                    if (locMeta.Rules.Where(r => r is ValueRule).Any())
                        locs.Add("", locMeta.Rules.Where(r => r is ValueRule).Select(r => r as ValueRule).First().Value);

                    locMeta.Rules.Where(r => r.Name.StartsWith("subtype[")).Select(r => r as GroupRule).ToList()
                        .ForEach(s => {
                            if (s.Rules.Where(r => r is ValueRule).Any())
                                locs.Add(s.Name.Replace("subtype[", "").Replace("]", ""), s.Rules.Where(r => r is ValueRule).Select(r => r as ValueRule).First().Value);
                        });
                }

                foreach (NodeModel node in values)
                {
                    ComplexTypeValues newValue;
                    string subtypePath = "";

                    if (!rule.MatchesType(node))
                        continue;

                    if (node.Name.Equals("despotic_monarchy"))
                    {

                    }

                    var subtypesMatches = subTypes.Where(s => s.MatchesSubtype(node));

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

                    string locaKey = "";
                    if (locs.ContainsKey(""))
                        locaKey = locs[""];
                    else if (subtypesMatches.Any() && locs.ContainsKey(subtypesMatches.First().Name))
                        locaKey = subtypesMatches.First().Name;

                    if (!String.IsNullOrWhiteSpace(locaKey))
                    {
                        if (locaKey.Contains("$"))
                        {
                            locaKey = locaKey.Replace("$", newValue.Key);
                        }

                        else
                        {
                            locaKey = ((node as GroupNodeModel).Nodes.Where(n => n.Name.Equals(locaKey)).First() as ValueNodeModel).Value;
                        }
                    }
                    else
                    {
                        locaKey = node.Name;
                    }

                    if (localisations.ContainsKey(locaKey))
                        newValue.Localisation = localisations[locaKey];

                    TypesValues[rule.Name][subtypePath].Add(newValue);
                }
            }
        }

        private Dictionary<string, string> ReadAllLocalisation()
        {
            Dictionary<string, string> localisations = new Dictionary<string, string>();

            string vanillaFolder = @"G:\Steam\steamapps\common\Europa Universalis IV";
            string modFolder = @"C:\Users\Mati\Documents\Paradox Interactive\Europa Universalis IV\mod\Anbennar-PublicFork";

            string[] folders = new string[] { vanillaFolder, modFolder };

            for (int i = 0; i < folders.Length; i++)
            {
                string[] files = Directory.GetFiles(Path.Combine(folders[i], "localisation"), ".yml");

                foreach (string file in files)
                {
                    using (FileStream fileStream = new FileStream(file, FileMode.Open))
                    {
                        Localisation.Read(new StreamReader(fileStream)).ForEach(tuple =>
                        {
                            if (!localisations.ContainsKey(tuple.Item1))
                                localisations.Add(tuple.Item1, tuple.Item2);
                            else
                                localisations[tuple.Item1] = tuple.Item2;
                        });
                    }
                }
            }

            return localisations;
        }
    }
}
//start_from_root
