using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using EMT.Helpers;
using EMT.Services;
using EMT.ViewModels;
using EMT.Views;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Threading.Tasks;

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
            // Unexpected errors are logged and shown instead of closing the app with unsaved work
            Dispatcher.UIThread.UnhandledException += (_, e) =>
            {
                Log.Error(e.Exception, "Unhandled exception");
                e.Handled = true;
                _ = Dialogs.ShowError("Something went wrong", $"{e.Exception.Message}\n\nYour changes are still in the tool, try saving.");
            };
            TaskScheduler.UnobservedTaskException += (_, e) =>
            {
                Log.Error(e.Exception, "Unobserved task exception");
                e.SetObserved();
            };

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
