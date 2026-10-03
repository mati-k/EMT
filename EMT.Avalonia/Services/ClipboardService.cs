using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using System.Threading.Tasks;

namespace EMT.Services
{
    public class ClipboardService : IClipboardService
    {
        private readonly IClassicDesktopStyleApplicationLifetime _desktop;

        public ClipboardService(IClassicDesktopStyleApplicationLifetime desktop)
        {
            _desktop = desktop;
        }

        public async Task SetTextAsync(string text)
        {
            var clipboard = _desktop.MainWindow?.Clipboard;
            if (clipboard != null)
            {
                await clipboard.SetTextAsync(text);
            }
        }
    }
}
