using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EMT.Helpers
{
    /// <summary>
    /// Finds where the game and the user's mods usually are, on Windows, Linux and macOS.
    /// </summary>
    public static partial class GamePaths
    {
        private const string GameFolderName = "Europa Universalis IV";

        [GeneratedRegex(@"""path""\s+""(?<path>[^""]+)""")]
        private static partial Regex LibraryPath();

        /// <summary>
        /// EU4 installed through Steam, in any of the Steam libraries. Null if not found.
        /// </summary>
        public static string? FindGameFolder()
        {
            try
            {
                return SteamLibraries()
                    .Select(library => Path.Combine(library, "steamapps", "common", GameFolderName))
                    .FirstOrDefault(IsGameFolder);
            }
            catch (Exception e)
            {
                Log.Warning(e, "Looking for game folder");
                return null;
            }
        }

        /// <summary>
        /// Folder with local mods, e.g. Documents/Paradox Interactive/Europa Universalis IV/mod. Null if it doesn't exist.
        /// </summary>
        public static string? FindModsFolder()
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string[] candidates = OperatingSystem.IsLinux()
                ? [Path.Combine(home, ".local", "share", "Paradox Interactive", GameFolderName, "mod")]
                : [Path.Combine(documents, "Paradox Interactive", GameFolderName, "mod"), Path.Combine(home, "Documents", "Paradox Interactive", GameFolderName, "mod")];

            return candidates.FirstOrDefault(Directory.Exists);
        }

        /// <summary>
        /// Folder where Steam installs games, to start the game folder picker in when detection fails.
        /// </summary>
        public static string? FindSteamCommonFolder()
        {
            try
            {
                return SteamLibraries().Select(library => Path.Combine(library, "steamapps", "common")).FirstOrDefault(Directory.Exists);
            }
            catch (Exception e)
            {
                Log.Warning(e, "Looking for Steam folder");
                return null;
            }
        }

        public static bool IsGameFolder(string? path)
        {
            return !string.IsNullOrWhiteSpace(path) && Directory.Exists(path)
                && PathHelper.ResolveCaseInsensitive(path, "interface") != null
                && PathHelper.ResolveCaseInsensitive(path, "missions") != null;
        }

        private static IEnumerable<string> SteamLibraries()
        {
            HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);

            foreach (string steam in SteamRoots().Where(Directory.Exists))
            {
                // Libraries listed by Steam (also on other drives) go first, they have proper casing unlike the registry
                foreach (string file in new[] { Path.Combine(steam, "steamapps", "libraryfolders.vdf"), Path.Combine(steam, "config", "libraryfolders.vdf") })
                {
                    if (!File.Exists(file))
                        continue;

                    foreach (Match match in LibraryPath().Matches(File.ReadAllText(file)))
                    {
                        string library = match.Groups["path"].Value.Replace(@"\\", @"\");
                        if (Directory.Exists(library) && seen.Add(Path.GetFullPath(library)))
                            yield return library;
                    }
                }

                if (seen.Add(Path.GetFullPath(steam)))
                    yield return steam;
            }
        }

        private static IEnumerable<string> SteamRoots()
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (OperatingSystem.IsWindows())
            {
                if (Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) is string userPath)
                    yield return userPath.Replace('/', '\\');
                if (Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", null) is string machinePath)
                    yield return machinePath;

                yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam");
            }
            else if (OperatingSystem.IsMacOS())
            {
                yield return Path.Combine(home, "Library", "Application Support", "Steam");
            }
            else
            {
                yield return Path.Combine(home, ".steam", "steam");
                yield return Path.Combine(home, ".local", "share", "Steam");
                yield return Path.Combine(home, ".var", "app", "com.valvesoftware.Steam", ".local", "share", "Steam");
                yield return Path.Combine(home, "snap", "steam", "common", ".local", "share", "Steam");
            }
        }
    }
}
