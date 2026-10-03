using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using EMT.Helpers;
using EMT.Models;
using EMT.Models.Gfx;
using Pdoxcl2Sharp;
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
                    GfxFileModel gfxFileData;
                    using (FileStream fileStream = File.OpenRead(gfxFile))
                    {
                        gfxFileData = ParadoxParser.Parse(fileStream, new GfxFileModel());
                    }

                    if (Path.GetFileName(gfxFile).Equals("core.gfx", StringComparison.OrdinalIgnoreCase) && _textColors.Count == 0)
                    {
                        LoadTextColors(gfxFileData);
                    }

                    foreach (GfxModel gfx in gfxFileData.Gfx)
                    {
                        if (gfx.Name == null || gfx.TextureFile == null || _missionGfx.ContainsKey(gfx.Name))
                            continue;

                        string texture = PathHelper.NormalizeGamePath(gfx.TextureFile);
                        string texturePath = PathHelper.ResolveCaseInsensitive(root, texture) ?? Path.Combine(root, texture);

                        if (texture.StartsWith(_missionGfxPrefix, StringComparison.OrdinalIgnoreCase))
                            _missionGfx.Add(gfx.Name, texturePath);

                        if (gfx.Name.Equals(_missionFrameGfx) && MissionFramePath == null)
                            MissionFramePath = texturePath;
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Loading gfx {GfxFile}", gfxFile);
                }
            }
        }

        private void LoadTextColors(GfxFileModel gfxFileData)
        {
            var colors = gfxFileData.OtherGfx.FirstOrDefault(b => b.Name.Equals("bitmapfonts"))
                ?.Nodes.FirstOrDefault(n => n.Name.Equals("textcolors"))?.Nodes;

            if (colors == null)
                return;

            foreach (var color in colors)
            {
                _textColors.Add(new ColorKey(color.Name[0], color.Colors));
            }
        }

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
