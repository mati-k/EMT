using Avalonia.Media;
using Avalonia.Media.Imaging;
using EMT.Models;
using System.Collections.Generic;

namespace EMT.Services
{
    public interface IGfxService
    {
        /// <summary>
        /// Mission sprites by name, mod sprites taking precedence over vanilla.
        /// </summary>
        public IReadOnlyDictionary<string, GfxSprite> MissionGfx { get; }
        public IReadOnlyList<ColorKey> TextColors { get; }
        public string? MissionFramePath { get; }

        public void Load(string vanillaFolder, string modFolder);
        public Bitmap? GetBitmap(string? filePath);
        public Bitmap? GetGfxBitmap(string? gfxName);
        public GfxSprite? GetSprite(string? gfxName);

        /// <summary>
        /// Frames of a sprite, single one for static sprites, empty if not found.
        /// </summary>
        public IReadOnlyList<IImage> GetFrames(string? gfxName);
        public IBrush? GetColorForKey(char key);

        /// <summary>
        /// Patch of the in-game mission window background, meant to be tiled. Null if the texture isn't found.
        /// </summary>
        public Bitmap? GetMissionBackgroundTile();
    }
}
