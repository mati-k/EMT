using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using EMT.Helpers;
using EMT.Models;
using EMT.Helpers.Script;
using Pfim;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace EMT.Services
{
    public class GfxService : IGfxService
    {
        private const string _missionGfxPrefix = "gfx/interface/missions";
        private const string _missionFrameGfx = "GFX_mission_icons_frame";

        private readonly Dictionary<string, string> _missionGfx = [];
        private readonly List<ColorKey> _textColors = [];
        private readonly Dictionary<string, Bitmap?> _bitmapCache = [];

        public IReadOnlyDictionary<string, string> MissionGfx => _missionGfx;
        public IReadOnlyList<ColorKey> TextColors => _textColors;
        public string? MissionFramePath { get; private set; }

        public void Load(string vanillaFolder, string modFolder)
        {
            _missionGfx.Clear();
            _textColors.Clear();
            MissionFramePath = null;

            // Mod first, so its definitions take precedence over vanilla ones
            foreach (string root in new[] { modFolder, vanillaFolder })
            {
                LoadRoot(PathHelper.NormalizeFolder(root));
            }
        }

        private void LoadRoot(string root)
        {
            string? interfaceFolder = PathHelper.ResolveCaseInsensitive(root, "interface");
            if (interfaceFolder == null)
                return;

            var gfxFiles = Directory.EnumerateFiles(interfaceFolder, "*.gfx", new EnumerationOptions
            {
                RecurseSubdirectories = true,
                MatchCasing = MatchCasing.CaseInsensitive,
            });

            foreach (string gfxFile in gfxFiles)
            {
                try
                {
                    ScriptNode gfxFileData = ScriptParser.Parse(TextFile.ReadScript(gfxFile).Text);

                    if (Path.GetFileName(gfxFile).Equals("core.gfx", StringComparison.OrdinalIgnoreCase) && _textColors.Count == 0)
                    {
                        LoadTextColors(gfxFileData);
                    }

                    foreach (ScriptNode sprite in Descendants(gfxFileData).Where(group => group.Name.Equals("spriteType", StringComparison.OrdinalIgnoreCase)))
                    {
                        string? name = Property(sprite, "name");
                        string? textureFile = Property(sprite, "texturefile");
                        if (name == null || textureFile == null || _missionGfx.ContainsKey(name))
                            continue;

                        string texture = PathHelper.NormalizeGamePath(textureFile);
                        string texturePath = PathHelper.ResolveCaseInsensitive(root, texture) ?? Path.Combine(root, texture);

                        if (texture.StartsWith(_missionGfxPrefix, StringComparison.OrdinalIgnoreCase))
                            _missionGfx.Add(name, texturePath);

                        if (name.Equals(_missionFrameGfx) && MissionFramePath == null)
                            MissionFramePath = texturePath;
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Loading gfx {GfxFile}", gfxFile);
                }
            }
        }

        private void LoadTextColors(ScriptNode gfxFileData)
        {
            var colors = gfxFileData.Child("bitmapfonts")?.Child("textcolors")?.Children;
            if (colors == null)
                return;

            foreach (ScriptNode color in colors.Where(color => color.IsGroup && color.Name.Length > 0))
            {
                _textColors.Add(new ColorKey(color.Name[0], color.Children!.Select(rgb => rgb.Name).ToList()));
            }
        }

        private static IEnumerable<ScriptNode> Descendants(ScriptNode group)
        {
            foreach (ScriptNode child in group.Children!.Where(child => child.IsGroup))
            {
                yield return child;
                foreach (ScriptNode descendant in Descendants(child))
                    yield return descendant;
            }
        }

        private static string? Property(ScriptNode group, string name) =>
            group.Children!.FirstOrDefault(node => !node.IsGroup && !node.IsBare && node.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value;

        public Bitmap? GetGfxBitmap(string? gfxName)
        {
            if (string.IsNullOrWhiteSpace(gfxName) || !_missionGfx.TryGetValue(gfxName, out string? path))
                return null;

            return GetBitmap(path);
        }

        public Bitmap? GetBitmap(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return null;

            if (_bitmapCache.TryGetValue(filePath, out Bitmap? cached))
                return cached;

            Bitmap? bitmap = LoadDds(filePath);
            _bitmapCache[filePath] = bitmap;
            return bitmap;
        }

        private static Bitmap? LoadDds(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            try
            {
                using var image = Pfimage.FromFile(filePath);
                GCHandle handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
                try
                {
                    return new Bitmap(PixelFormat(image), AlphaFormat.Unpremul, handle.AddrOfPinnedObject(),
                        new PixelSize(image.Width, image.Height), new Vector(96, 96), image.Stride);
                }
                finally
                {
                    handle.Free();
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Loading picture {FilePath}", filePath);
                return null;
            }
        }

        private static PixelFormat PixelFormat(Pfim.IImage image)
        {
            return image.Format switch
            {
                ImageFormat.Rgb24 => PixelFormats.Bgr24,
                ImageFormat.Rgba32 => PixelFormats.Bgra8888,
                ImageFormat.Rgb8 => PixelFormats.Gray8,
                ImageFormat.R5g5b5a1 => PixelFormats.Bgr555,
                ImageFormat.R5g5b5 => PixelFormats.Bgr555,
                ImageFormat.R5g6b5 => PixelFormats.Bgr565,
                _ => throw new Exception($"Unable to convert {image.Format} to Avalonia PixelFormat"),
            };
        }

        public IBrush? GetColorForKey(char key)
        {
            return _textColors.FirstOrDefault(color => color.Key == key)?.Brush;
        }
    }
}
