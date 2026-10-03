using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using EMT.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EMT.Views
{
    public partial class ConfigurationView : UserControl
    {
        private static FilePickerFileType MissionFiles { get; } = new("Mission files") { Patterns = ["*.txt"] };
        private static FilePickerFileType LocalisationFiles { get; } = new("Localisation files") { Patterns = ["*.yml"] };

        private ConfigurationViewModel ViewModel => (ConfigurationViewModel)DataContext!;

        public ConfigurationView()
        {
            InitializeComponent();
        }

        private async void MissionFile_Click(object? sender, RoutedEventArgs e)
        {
            string? path = await PickFile("Select Mission File", MissionFiles, StartHints(ViewModel.MissionFile, "missions"));
            if (path != null)
                ViewModel.MissionFile = path;
        }

        private async void NewMissionFile_Click(object? sender, RoutedEventArgs e)
        {
            string? path = await CreateFile("New Mission File", MissionFiles, ".txt", StartHints(ViewModel.MissionFile, "missions"));
            if (path != null)
                ViewModel.MissionFile = path;
        }

        private async void LocalisationFile_Click(object? sender, RoutedEventArgs e)
        {
            string? path = await PickFile("Select Localisation File", LocalisationFiles, StartHints(ViewModel.LocalisationFile, "localisation"));
            if (path != null)
                ViewModel.LocalisationFile = path;
        }

        private async void NewLocalisationFile_Click(object? sender, RoutedEventArgs e)
        {
            string? path = await CreateFile("New Localisation File", LocalisationFiles, ".yml", StartHints(ViewModel.LocalisationFile, "localisation"));
            if (path != null)
                ViewModel.LocalisationFile = path;
        }

        private async void VanillaFolder_Click(object? sender, RoutedEventArgs e)
        {
            string? path = await PickFolder("Select Europa Universalis IV folder", ViewModel.VanillaFolder);
            if (path != null)
                ViewModel.VanillaFolder = path;
        }

        private async void ModFolder_Click(object? sender, RoutedEventArgs e)
        {
            string? path = await PickFolder("Select mod folder", ViewModel.ModFolder);
            if (path != null)
                ViewModel.ModFolder = path;
        }

        /// <summary>
        /// Folders to try opening pickers in: next to current file, then the matching mod subfolder.
        /// </summary>
        private List<string?> StartHints(string currentFile, string modSubfolder)
        {
            List<string?> hints = [];

            if (!string.IsNullOrWhiteSpace(currentFile))
                hints.Add(Path.GetDirectoryName(currentFile));

            if (!string.IsNullOrWhiteSpace(ViewModel.ModFolder))
            {
                hints.Add(Path.Combine(ViewModel.ModFolder, modSubfolder));
                hints.Add(ViewModel.ModFolder);
            }

            return hints;
        }

        private async Task<IStorageFolder?> FirstExistingFolder(IStorageProvider storageProvider, IEnumerable<string?> hints)
        {
            foreach (string? hint in hints)
            {
                if (!string.IsNullOrWhiteSpace(hint) && Directory.Exists(hint)
                    && await storageProvider.TryGetFolderFromPathAsync(hint) is IStorageFolder folder)
                {
                    return folder;
                }
            }

            return null;
        }

        private async Task<string?> PickFile(string title, FilePickerFileType filter, List<string?> hints)
        {
            var storageProvider = TopLevel.GetTopLevel(this)!.StorageProvider;

            var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = [filter, FilePickerFileTypes.All],
                SuggestedStartLocation = await FirstExistingFolder(storageProvider, hints),
            });

            return files.Count >= 1 ? files[0].TryGetLocalPath() : null;
        }

        private async Task<string?> CreateFile(string title, FilePickerFileType filter, string extension, List<string?> hints)
        {
            var storageProvider = TopLevel.GetTopLevel(this)!.StorageProvider;

            var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = title,
                DefaultExtension = extension,
                FileTypeChoices = [filter],
                ShowOverwritePrompt = true,
                SuggestedStartLocation = await FirstExistingFolder(storageProvider, hints),
            });

            string? path = file?.TryGetLocalPath();
            // Never truncate an existing file picked through the overwrite prompt
            if (path != null && !File.Exists(path))
                File.WriteAllText(path, "");

            return path;
        }

        private async Task<string?> PickFolder(string title, string current)
        {
            var storageProvider = TopLevel.GetTopLevel(this)!.StorageProvider;

            var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                SuggestedStartLocation = await FirstExistingFolder(storageProvider, [current]),
            });

            return folders.Count >= 1 ? folders[0].TryGetLocalPath() : null;
        }
    }
}
