using Caliburn.Micro;
using EMT.Handlers;
using EMT.Models;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

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
            FilesModel.MissionFile = SelectFile("txt File", "*.txt");
        }

        public void SelectLocalisationFile()
        {
            FilesModel.LocalisationFile = SelectFile("yml File", "*.yml");
        }

        public void AddGFXFile()
        {
            string file = SelectFile("gfx File", "*.gfx");
            if (!string.IsNullOrEmpty(file))
                FilesModel.GFXFiles.Add(file);
        }

        public void RemoveGFXFile(string file)
        {
            FilesModel.GFXFiles.Remove(file);
        }

        private string SelectFile(string extensionTitle, string extension)
        {
            //Microsoft.Win32.file
            //Microsoft.Win32.OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;

            string filter = extension;
            openFileDialog.Filter = extensionTitle + "|" + extension;

            if (openFileDialog.ShowDialog() == true)
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
