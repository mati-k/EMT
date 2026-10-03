using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using EMT.Services;
using EMT.ViewModels;
using EMT.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace EMT
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // Pdoxcl2Sharp reads game files as Windows-1252
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var services = new ServiceCollection();
                services.AddSingleton<IConfigService, ConfigService>();
                services.AddSingleton<IGfxService, GfxService>();
                services.AddSingleton<IClipboardService>(x => new ClipboardService(desktop));
                Ioc.Default.ConfigureServices(services.BuildServiceProvider());

                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
