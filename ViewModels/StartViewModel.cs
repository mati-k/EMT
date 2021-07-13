using Caliburn.Micro;
using EMT.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMT.ViewModels
{
    public class StartViewModel : Screen
    {
        private IEventAggregator _eventAggregator;
        private FilesModel _filesModel;

        public FilesModel FilesModel
        {
            get { return _filesModel; }
            set
            {
                _filesModel = value;
                NotifyOfPropertyChange(() => FilesModel);
            }
        }

        public StartViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            FilesModel = EMT.Models.FilesModel.ReadFromJson();
        }

        public void SelectMissionFile()
        {
            FilesModel.MissionFile = SelectFile("txt File", ".txt");
        }

        public void SelectLocalisationFile()
        {
            FilesModel.LocalisationFile = SelectFile("yml File", ".yml");
        }

        public void AddGFXFile()
        {
            string file = SelectFile("gfx File", ".gfx");
            if (!string.IsNullOrEmpty(file))
                FilesModel.GFXFiles.Add(file);
        }

        public void RemoveGFXFile(string file)
        {
            FilesModel.GFXFiles.Remove(file);
        }

        private string SelectFile(string extensionTitle, string extension)
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();
            openFileDialog.Multiselect = false;

            CommonFileDialogFilter filter = new CommonFileDialogFilter(extensionTitle, extension);
            openFileDialog.Filters.Add(filter);

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return openFileDialog.FileName;
            }

            return "";
        }

        public bool CanContinue(string filesModel_MissionFile, string filesModel_LocalisationFile)
        {
            return !String.IsNullOrWhiteSpace(filesModel_MissionFile) && !String.IsNullOrWhiteSpace(filesModel_LocalisationFile);
        }

        public void Continue(string filesModel_MissionFile, string filesModel_LocalisationFile)
        {
            FilesModel.SaveToJson();
            _eventAggregator.PublishOnUIThreadAsync(FilesModel);
        }
    }
}
