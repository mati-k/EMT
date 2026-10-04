using System;
using System.IO;
using System.Linq;

namespace EMT.Helpers
{
    public static class PathHelper
    {
        /// <summary>
        /// Converts game-style paths ("gfx//interface\missions/x.dds") to "gfx/interface/missions/x.dds".
        /// </summary>
        public static string NormalizeGamePath(string path)
        {
            path = path.Replace('\\', '/');
            while (path.Contains("//"))
                path = path.Replace("//", "/");

            return path.TrimStart('/');
        }

        /// <summary>
        /// Resolves a relative game path under root, ignoring case. Game files reference paths
        /// with inconsistent casing, which only works out of the box on Windows.
        /// </summary>
        public static string? ResolveCaseInsensitive(string root, string relativePath)
        {
            string[] segments = NormalizeGamePath(relativePath).Split('/', StringSplitOptions.RemoveEmptyEntries);
            string exact = Path.Combine([root, .. segments]);
            if (File.Exists(exact) || Directory.Exists(exact))
                return exact;

            string current = root;
            foreach (string segment in segments)
            {
                if (!Directory.Exists(current))
                    return null;

                string? match = Directory.EnumerateFileSystemEntries(current)
                    .FirstOrDefault(entry => string.Equals(Path.GetFileName(entry), segment, StringComparison.OrdinalIgnoreCase));

                if (match == null)
                    return null;

                current = match;
            }

            return current;
        }

        public static string NormalizeFolder(string folder)
        {
            return Path.TrimEndingDirectorySeparator(Path.GetFullPath(folder));
        }
    }
}
