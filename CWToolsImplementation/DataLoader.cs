using CWTools.Common;
using CWTools.Games;
using CWTools.Rules;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CWTools.Utilities;
using CWTools.Games.EU4;
using MBrace.CsPickler;

namespace EMT.CWToolsImplementation
{
    public class DataLoader
    {
        //private const string configFolder = @"cwtools-eu4-config";
        private const string configFolder = @"E:\Moje\EMT_CONFIG\EMT_CONFIG\cwtools-eu4-config";

        private string vanillaFolder;
        private string modFolder;

        public SavedModData SavedData { get; set; }
        public List<RootRule> Rules { get; set; }

        public DataLoader(string vanillaFolder, string modFolder)
        {
            this.vanillaFolder = vanillaFolder;
            this.modFolder = modFolder;
        }

        public void Load(bool forceReload = false)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (!LoadSavedGameData())
                LoadNewModData();

            var (rules, _, _, _, _) = CWTools.CSharp.Helpers.LoadAndInitializeFromConfigFiles(GetConfigFiles(), Game.STL);
            Rules = ListModule.ToSeq(rules).ToList();
        }

        private bool LoadSavedGameData()
        {
            string saveName = string.Format(@"saved_{0}.cwb", Path.GetFileName(modFolder));
            if (!File.Exists(saveName))
                return false;

            byte[] save = File.ReadAllBytes(saveName);
            SavedData = CsPickler.CreateBinarySerializer().UnPickle<SavedModData>(save);

            var folders = Utils.getAllFoldersUnion(new List<string> { configFolder });
            var files = folders.SelectMany(folder => Directory.EnumerateFiles(folder))
                                    .Where(f => Path.GetExtension(f) == ".cwt" || Path.GetExtension(f) == ".log")
                                    .Select(f => Tuple.Create(f, File.ReadAllText(f)));

            return true;
        }

        private void LoadNewModData()
        {
            EmbeddedSetupSettings embeddedSetupSettings = GetEmbeddedSetupSettings();

            ValidationSettings validationSettings = new ValidationSettings(ListModule.OfSeq(new List<Lang>() { Lang.NewEU4(EU4Lang.English) }), true, false);

            List<Files.WorkspaceDirectoryInput> rootDirectories = new List<Files.WorkspaceDirectoryInput>() { Files.WorkspaceDirectoryInput.NewWD(new Files.WorkspaceDirectory(modFolder, Path.GetFileName(modFolder))) };

            GameSetupSettings<EU4Lookup> gameSettings = new GameSetupSettings<EU4Lookup>(
                ListModule.OfSeq(rootDirectories),
                embeddedSetupSettings,
                validationSettings,
                new RulesSettings(ListModule.OfSeq(GetConfigFiles()), true, false, false),
                EU4Constants.scriptFolders,
                FSharpOption<FSharpList<string>>.None,
                FSharpOption<string>.None,
                FSharpOption<int>.None
                );

            IGame<EU4ComputedData> eu4Game = new EU4Game(gameSettings);

            SavedData = new SavedModData(eu4Game.Types(), eu4Game.GetEmbeddedMetadata(), eu4Game.ScriptedEffects(), eu4Game.ScriptedTriggers());

            byte[] save = BinarySerializer.Create().Pickle<SavedModData>(SavedData);
            File.WriteAllBytes(string.Format(@"saved_{0}.cwb", Path.GetFileName(modFolder)), save);
        }

        private EmbeddedSetupSettings GetEmbeddedSetupSettings()
        {
            if (File.Exists("vanilla.cwb"))
            {
                CachedResourceData cached = CsPickler.CreateBinarySerializer().UnPickle<CachedResourceData>(File.ReadAllBytes("vanilla.cwb"));
                Position.fileIndexTable = cached.fileIndexTable;
                StringResource.stringManager = cached.stringResourceManager;
                return EmbeddedSetupSettings.NewFromConfig(cached.files, cached.resources);
            }

            List<Files.WorkspaceDirectoryInput> vanillaDir = new List<Files.WorkspaceDirectoryInput>() { Files.WorkspaceDirectoryInput.NewWD(new Files.WorkspaceDirectory(vanillaFolder, "Europa Universalis IV")) };
            var fileManager = new Files.FileManager(ListModule.OfSeq(vanillaDir), FSharpOption<string>.Some(""), EU4Constants.scriptFolders, "Europa Universalis IV", Encoding.UTF8, FSharpList<string>.Empty, 8);

            var resources = new ResourceManager<EU4ComputedData>(
                FSharpFunc<Entity, EU4ComputedData>.FromConverter(new Converter<Entity, EU4ComputedData>(ComputeDataFun)),
                FSharpFunc<Entity, FSharpFunc<EU4ComputedData, Unit>>.FromConverter(new Converter<Entity, FSharpFunc<EU4ComputedData, Unit>>(ComputeDataUpdateFun)),
                Encoding.GetEncoding(1252),
                Encoding.UTF8).Api;

            var entities = resources.UpdateFiles.Invoke(fileManager.AllFilesByPath())
                .Where((r) => r.Item2 != FSharpOption<(Entity, Lazy<EU4ComputedData>)>.None)
                .Select(r => new Tuple<Resource, Entity>(r.Item1, r.Item2.Value.Item1));

            CachedResourceData newCached = new CachedResourceData(ListModule.OfSeq(entities), AllVanillaFiles(), Position.fileIndexTable, StringResource.stringManager);
            // serialization of lazy problem
            //byte[] save = BinarySerializer.Create().Pickle(newCached);
            //File.WriteAllBytes("vanilla.cwb", save);

            return EmbeddedSetupSettings.NewFromConfig(AllVanillaFiles(), ListModule.OfSeq(entities));
        }

        private FSharpList<Tuple<string, string>> AllVanillaFiles()
        {
            List<Tuple<string, string>> fileContents = new List<Tuple<string, string>>();

            foreach (string folder in EU4Constants.scriptFolders)
            {
                if (!Directory.Exists(Path.Combine(vanillaFolder, folder)))
                    continue;

                foreach (string file in Directory.GetFiles(Path.Combine(vanillaFolder, folder)))
                {
                    fileContents.Add(new Tuple<string, string>(file, File.ReadAllText(file)));
                }
            }

            return ListModule.OfSeq(fileContents);
        }

        private List<Tuple<string, string>> GetConfigFiles()
        {
            var folders = Utils.getAllFoldersUnion(new List<string> { configFolder });
            var files = folders.SelectMany(folder => Directory.EnumerateFiles(folder))
                                    .Where(f => Path.GetExtension(f) == ".cwt" || Path.GetExtension(f) == ".log")
                                    .Select(f => Tuple.Create(f, File.ReadAllText(f)));

            return files.ToList();
        }

        private FSharpOption<InfoService> ComputeFun(Unit unit)
        {
            return FSharpOption<InfoService>.None;
        }

        private EU4ComputedData ComputeDataFun(Entity entity)
        {
            return Compute.EU4.computeEU4Data(FSharpFunc<Unit, FSharpOption<InfoService>>.FromConverter(new Converter<Unit, FSharpOption<InfoService>>(ComputeFun)), entity);
        }

        private FSharpFunc<EU4ComputedData, Unit> ComputeDataUpdateFun(Entity entity)
        {
            return FuncConvert.FromAction<EU4ComputedData>((computed) =>
                Compute.EU4.computeEU4DataUpdate(FSharpFunc<Unit, FSharpOption<InfoService>>.FromConverter(ComputeFun), entity, computed));
        }
    }
}
