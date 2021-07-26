using Caliburn.Micro;
using EMT.Handlers;
using EMT.Models;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Dialogs;

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

        public IDropTarget DropHandler { get; } = new DropTargetHandler();

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

        public void SelectVanillaFolder()
        {
            FilesModel.VanillaFolder = SelectFolder();
        }

        public void SelectModFolder()
        {
            FilesModel.ModFolder = SelectFolder();
        }

        private string SelectFile(string extensionTitle, string extension)
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();
            openFileDialog.Multiselect = false;

            openFileDialog.Filters.Add(new CommonFileDialogFilter(extensionTitle, extension));

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return openFileDialog.FileName;
            }

            return "";
        }

        public string SelectFolder()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.IsFolderPicker = true;

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return openFileDialog.FileName;
            }

            return "";
        }

        public void AddValue(GroupNodeModel node)
        {
            node.Nodes.Add(new ValueNodeModel() { Parent = node }); ;
        }

        public void AddGroup(GroupNodeModel node)
        {
            node.Nodes.Add(new GroupNodeModel() { Parent = node });
        }

        public void RemoveValue(ValueNodeModel node)
        {
            node.Parent.Nodes.Remove(node);
        }

        public void RemoveGroup(GroupNodeModel node)
        {
            node.Parent.Nodes.Remove(node);
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
