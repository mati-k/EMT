using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.DependencyInjection;
using EMT.Services;

namespace EMT.Converters
{
    public static class DDSConverter
    {
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
