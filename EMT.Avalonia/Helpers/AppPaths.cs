using System;
using System.IO;

namespace EMT.Helpers
{
    /// <summary>
    /// Per-user locations for config and logs. The working directory isn't reliable (e.g. a macOS .app bundle starts in "/").
    /// </summary>
    public static class AppPaths
    {
        public static string DataFolder { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EMT");
        public static string ConfigFile { get; } = Path.Combine(DataFolder, "config.json");
        public static string LogFolder { get; } = Path.Combine(DataFolder, "logs");

        /// <summary>
        /// Config file written by the WPF version, next to the executable or in the working directory.
        /// </summary>
        public static string[] LegacyConfigFiles { get; } =
        [
            Path.Combine(AppContext.BaseDirectory, "paths.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "paths.json"),
        ];
    }
}
