using Caliburn.Micro;
using EMT.Converters;
using EMT.Models;
using EMT.Views;
using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using EMT.SharedData;
using System.Windows.Media;

namespace EMT.ViewModels
{
    public class ShellViewModel : Conductor<object>, IHandle<FilesModel>
    {
        private FilesModel _filesModel;
        private IEventAggregator _eventAggregator;
        private StartViewModel _startViewModel;
        private MissionViewModel _missionViewModel;
        private MissionFileModel _missionFile;
        private Dictionary<string, string> _unconnectedLocalisation = new Dictionary<string, string>();

        private string configPath = "cwtools-eu4-config";

        public MissionFileModel MissionFile
        {
            get { return _missionFile; }
            set 
            {
                _missionFile = value;
                NotifyOfPropertyChange(() => MissionFile);
            }
        }

        public ShellViewModel(IEventAggregator eventAggregator, StartViewModel startViewModel, MissionViewModel missionViewModel)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);

            _startViewModel = startViewModel;
            _missionViewModel = missionViewModel;

            ActivateItemAsync(startViewModel, CancellationToken.None);
        }

        public void Save()
        {
            string backupName = _filesModel.MissionFile;
            while (File.Exists(backupName))
                backupName = backupName + "_copy";
            File.Copy(_filesModel.MissionFile, backupName);

            try
            {
                using (ParadoxStreamWriter writer = new ParadoxSaverCustom(new FileStream(_filesModel.MissionFile, FileMode.Create)))
                {
                    MissionFile.Write(writer);
                }
                File.Delete(backupName);
            } 
            
            catch (Exception e)
            {

            }

            backupName = _filesModel.LocalisationFile;
            while (File.Exists(backupName))
                backupName = backupName + "_copy";
            File.Copy(_filesModel.LocalisationFile, backupName);

            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(_filesModel.LocalisationFile, FileMode.Create), new UTF8Encoding(true)))
                {
                    Localisation.Write(writer, MissionFile);
                    writer.WriteLine();
                    foreach (string key in _unconnectedLocalisation.Keys)
                    {
                        writer.WriteLine(" {0}:0 \"{1}\"", key, _unconnectedLocalisation[key]);
                    }
                }
                File.Delete(backupName);
            }
            catch (Exception e)
            {

            }
        }

        public Task HandleAsync(FilesModel message, CancellationToken cancellationToken)
        {
            _filesModel = message;
            MissionFileModel missionFileModel;

            using (FileStream fileStream = new FileStream(message.MissionFile, FileMode.Open))
            {
                missionFileModel = ParadoxParser.Parse(fileStream, new MissionFileModel());
                missionFileModel.FileName = message.MissionFile;
            }

            using (FileStream fileStream = new FileStream(message.LocalisationFile, FileMode.Open))
            {
                Dictionary<string, string> localisation = new Dictionary<string, string>();
                Localisation.Read(new StreamReader(fileStream)).ForEach(tuple =>
                {
                    if (!localisation.ContainsKey(tuple.Item1))
                        localisation.Add(tuple.Item1, tuple.Item2);
                    else
                        Console.WriteLine("Duplicate localisation: {0} = {1}", tuple.Item1, tuple.Item2); // Save to file?
                });

                List<string> used = new List<string>();
                foreach (MissionBranchModel branch in missionFileModel.Branches)
                {
                    foreach (MissionModel mission in branch.Missions)
                    {
                        if (localisation.ContainsKey(mission.Name + "_title"))
                        {
                            mission.Title = localisation[mission.Name + "_title"];
                            used.Add(mission.Name + "_title");
                        }

                        if (localisation.ContainsKey(mission.Name + "_desc"))
                        {
                            mission.Description = localisation[mission.Name + "_desc"];
                            used.Add(mission.Name + "_desc");
                        }
                    }
                }
                
                foreach (string key in localisation.Keys)
                {
                    if (!used.Contains(key))
                        _unconnectedLocalisation.Add(key, localisation[key]);
                }
            }
            LoadGfx(message.VanillaFolder, message.ModFolder);

            MissionFile = missionFileModel;
            _eventAggregator.PublishOnUIThreadAsync(missionFileModel);

            return ActivateItemAsync(_missionViewModel, CancellationToken.None);
        }


        private void LoadGfx(string vanillaFolder, string modFolder)
        {
            Dictionary<string, string> gfxFiles = new Dictionary<string, string>();
            List<string> files = GetAllFolderAndSubfolders(new List<string>() { Path.Combine(modFolder, "interface"), Path.Combine(vanillaFolder, "interface") })
                                .SelectMany(folder => Directory.EnumerateFiles(folder)).Where(f => Path.GetExtension(f).Equals(".gfx")).ToList();

            foreach (string gfxFile in files)
            {
                using (FileStream fileStream = new FileStream(gfxFile, FileMode.Open))
                {
                    GfxFileModel gfxFileData = ParadoxParser.Parse(fileStream, new GfxFileModel());
                    string rootDirectory = gfxFile;
                    while (!(rootDirectory.Equals(modFolder) || rootDirectory.Equals(vanillaFolder)))
                        rootDirectory = Directory.GetParent(rootDirectory).FullName;

                    if (gfxFile.Contains("core.gfx") && !FontColors.Instance.Colors.Any()) // skip if mod added
                    {
                        var colors = gfxFileData.OtherGfx.Where(b => b.Name.Equals("bitmapfonts")).First()
                            .Nodes.Where(n => n.Name.Equals("textcolors")).First().Nodes;

                        foreach (var color in colors)
                        {
                            FontColors.Instance.Colors.Add(new ColorKey(color.Name[0], color.Colors));
                        }
                    }

                    gfxFileData.Gfx.ToList().ForEach(gfx=> {
                        if (gfx.TextureFile != null && !gfxFiles.ContainsKey(gfx.Name))
                        {
                            if (gfx.TextureFile.Replace(@"//", @"/").StartsWith("gfx/interface/missions"))
                                gfxFiles.Add(gfx.Name, Path.Combine(rootDirectory, gfx.TextureFile));
                            if (gfx.Name.Equals("GFX_mission_icons_frame"))
                                StaticPaths.Instance.MissionFramePath = Path.Combine(rootDirectory, gfx.TextureFile);
                        }
                    });
                }
            }
            
            GfxStorage.Instance.GfxFiles = gfxFiles;
        }

        private List<string> GetAllFolderAndSubfolders(List<string> rootFolders)
        {
            List<string> folders = new List<string>();

            foreach (string folder in rootFolders)
            {
                folders.Add(folder);

                List<string> subDirectories = Directory.GetDirectories(folder).ToList();
                if (subDirectories.Count > 0)
                {
                    folders.AddRange(GetAllFolderAndSubfolders(subDirectories));
                }
            }

            return folders;
        }
    }
}
