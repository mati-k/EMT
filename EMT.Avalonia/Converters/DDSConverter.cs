using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.DependencyInjection;
using EMT.Services;

namespace EMT.Converters
{
    public static class DDSConverter
    {
        /// <summary>
        /// Sprite name (e.g. GFX_mission_icon) to bitmap.
        /// </summary>
        public static FuncValueConverter<string?, Bitmap?> FromGfxName { get; } =
            new FuncValueConverter<string?, Bitmap?>(gfxName => Ioc.Default.GetService<IGfxService>()?.GetGfxBitmap(gfxName));

        /// <summary>
        /// Absolute .dds path to bitmap.
        /// </summary>
        public static FuncValueConverter<string?, Bitmap?> FromPath { get; } =
            new FuncValueConverter<string?, Bitmap?>(path => Ioc.Default.GetService<IGfxService>()?.GetBitmap(path));

        /// <summary>
        /// Ignores the bound value, returns the mission frame.
        /// </summary>
        public static FuncValueConverter<object?, Bitmap?> MissionFrame { get; } =
            new FuncValueConverter<object?, Bitmap?>(_ =>
            {
                var gfxService = Ioc.Default.GetService<IGfxService>();
                return gfxService?.GetBitmap(gfxService.MissionFramePath);
            });
    }
}
