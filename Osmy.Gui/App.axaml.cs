using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Osmy.Gui.Services;
using Osmy.Gui.ViewModels;
using Osmy.Gui.Views;
using System.Threading.Tasks;

namespace Osmy.Gui
{
    public partial class App : Application
    {
        private readonly BackgroundServiceManager _backgroundServiceManager = new();

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };

                _ = Task.Run(StartBackgroundServices);
            }

            RequestedThemeVariant = ThemeVariant.Light;

            base.OnFrameworkInitializationCompleted();
        }

        private async Task StartBackgroundServices()
        {
            // TODO ìKêÿÇ»èàóùÇ©åüì¢
            _backgroundServiceManager.Register(new VulnerabilityScanService(new AppNotificationService()));
            _backgroundServiceManager.Register(new ChecksumVerificationService(new AppNotificationService()));
            await _backgroundServiceManager.StartAsync().ConfigureAwait(false);
        }
    }
}
