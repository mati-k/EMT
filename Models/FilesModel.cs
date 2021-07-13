using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Newtonsoft.Json;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace EMT.Models
{
    [DataContract]
    public class FilesModel : PropertyChangedBase
    {
        private const string pathsFile = "paths.json";

        private string _missionFile;
        private string _localisationFile;
        public BindableCollection<string> _gfxFiles = new BindableCollection<string>();

        [DataMember]
        public string MissionFile
        {
            get { return _missionFile; }
            set
            {
                _missionFile = value;
                NotifyOfPropertyChange(() => MissionFile);
            }
        }

        [DataMember]
        public string LocalisationFile
        {
            get { return _localisationFile; }
            set
            {
                _localisationFile = value;
                NotifyOfPropertyChange(() => LocalisationFile);
            }
        }

        [DataMember]
        public BindableCollection<string> GFXFiles
        {
            get { return _gfxFiles; }
            set
            {
                _gfxFiles = value;
                NotifyOfPropertyChange(() => GFXFiles);
            }
        }

        public FilesModel()
        {

        }

        public FilesModel(string missionFile, string localizationFile, BindableCollection<string> gfxFiles)
        {
            this.MissionFile = missionFile;
            this.LocalisationFile = localizationFile;
            this.GFXFiles = gfxFiles;
        }

        public void SaveToJson()
        {
            string file = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(pathsFile, file);
        }

        public static FilesModel ReadFromJson()
        {
            try
            {
                string file = File.ReadAllText(pathsFile);
               
                FilesModel filesModel = JsonConvert.DeserializeObject<FilesModel>(file);
                
                return filesModel;
            }

            catch (Exception e)
            {
                return new FilesModel();
            }
        }
    }
}
