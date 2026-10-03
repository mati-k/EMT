using Avalonia.Media;
using Avalonia.Media.Imaging;
using EMT.Models;
using System.Collections.Generic;

namespace EMT.Services
{
    public interface IGfxService
    {
        /// <summary>
        /// Mission sprite name to absolute texture path, mod sprites taking precedence over vanilla.
        /// </summary>
        public IReadOnlyDictionary<string, string> MissionGfx { get; }
        public IReadOnlyList<ColorKey> TextColors { get; }
        public string? MissionFramePath { get; }

        public void Load(string vanillaFolder, string modFolder);
        public Bitmap? GetBitmap(string? filePath);
        public Bitmap? GetGfxBitmap(string? gfxName);
        public IBrush? GetColorForKey(char key);
    }
}
