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
using System.IO;

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
            string selected = SelectFile("txt File", ".txt", "missions");
            if (!string.IsNullOrWhiteSpace(selected))
                FilesModel.MissionFile = selected;
        }
        public void CreateMissionFile()
        {
            CommonSaveFileDialog save = new CommonSaveFileDialog();
            save.Filters.Add(new CommonFileDialogFilter("txt", ".txt"));
            save.DefaultExtension = ".txt";
            save.AlwaysAppendDefaultExtension = true;

            if (!string.IsNullOrWhiteSpace(FilesModel.ModFolder))
                save.InitialDirectory = Path.Combine(FilesModel.ModFolder, "missions");

            if (save.ShowDialog() == CommonFileDialogResult.Ok)
            {
                File.Create(save.FileName);
                FilesModel.MissionFile = save.FileName;
            }
        }

        public void SelectLocalisationFile()
        {
            string selected = SelectFile("yml File", ".yml", "localisation");
            if (!string.IsNullOrWhiteSpace(selected))
                FilesModel.LocalisationFile = selected;
        }

        public void CreateLocalisationFile()
        {
            CommonSaveFileDialog save = new CommonSaveFileDialog();
            save.Filters.Add(new CommonFileDialogFilter("yml", ".yml"));
            save.DefaultExtension = ".yml";
            save.AlwaysAppendDefaultExtension = true;

            if (!string.IsNullOrWhiteSpace(FilesModel.ModFolder))
                save.InitialDirectory = Path.Combine(FilesModel.ModFolder, "localisation");

            if (save.ShowDialog() == CommonFileDialogResult.Ok)
            {
                File.Create(save.FileName);
                FilesModel.LocalisationFile = save.FileName;
            }
        }

        public void SelectVanillaFolder()
        {
            string selected = SelectFolder();
            if (!string.IsNullOrWhiteSpace(selected))
                FilesModel.VanillaFolder = selected;
        }

        public void SelectModFolder()
        {
            string selected = SelectFolder();
            if (!string.IsNullOrWhiteSpace(selected))
                FilesModel.ModFolder = selected;
        }

        private string SelectFile(string extensionTitle, string extension, string subfolder)
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filters.Add(new CommonFileDialogFilter(extensionTitle, extension));

            if (!string.IsNullOrWhiteSpace(FilesModel.ModFolder))
                openFileDialog.InitialDirectory = Path.Combine(FilesModel.ModFolder, subfolder);

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
