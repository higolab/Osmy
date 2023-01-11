using CommandLine;
using Microsoft.EntityFrameworkCore;
using Osmy.Models;
using Osmy.Models.Sbom.Spdx;
using Osmy.Services;
using Osmy.ViewModels;
using Osmy.Views;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using DryIoc;
using Prism.Services.Dialogs;
using NLog.Extensions.Logging;
using Microsoft.Extensions.Logging;

namespace Osmy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private readonly BackgroundServiceManager _backgroundServiceManager = new();

        private ILogger Logger => _logger ??= CreateLogger();
        private ILogger? _logger;

        /// <summary>
        /// 起動オプション
        /// </summary>
        public LaunchOption? LaunchOption { get; private set; }

        protected override void Initialize()
        {
            Parser.Default.ParseArguments<LaunchOption>(Environment.GetCommandLineArgs())
                   .WithParsed(o => LaunchOption = o);

            base.Initialize();
        }

        protected override Window? CreateShell()
        {
            // バックグラウンド起動であれば画面を表示しない
            return LaunchOption?.IsBackground == true ? null : Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<DashboardView>();
            containerRegistry.RegisterForNavigation<SbomListView>();
            containerRegistry.RegisterForNavigation<SettingView>();
            containerRegistry.RegisterDialogWindow<MetroDialogWindow>();
            containerRegistry.RegisterDialog<AddSbomDialog, AddSbomDialogViewModel>();
            containerRegistry.RegisterDialog<MessageBoxDialog, MessageBoxDialogViewModel>();

            ViewModelLocationProvider.Register<DashboardView, DashboardViewViewModel>();
            ViewModelLocationProvider.Register<SbomListView, SbomListViewViewModel>();
            ViewModelLocationProvider.Register<SettingView, SettingViewViewModel>();

            //containerRegistry.Register<IAppNotificationService, AppNotificationService>();    // TODO

            containerRegistry.RegisterInstance(_backgroundServiceManager);
            containerRegistry.RegisterSingleton<IMessageBoxService>(() => new MessageBoxService(Container.Resolve<IDialogService>()));

            containerRegistry.RegisterInstance(Logger);
        }

        private void InitDb()
        {
            using var context = new ManagedSoftwareContext();
            var dir = Path.GetDirectoryName(context.DbPath);

            if (dir is not null)
            {
                Directory.CreateDirectory(dir);
            }

            context.Database.Migrate();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Current.Dispatcher.UnhandledException += Dispatcher_UnhandledException;

            await SpdxConverter.FetchConverterAsync();

            _backgroundServiceManager.Register(new NotifyIconService());
            _backgroundServiceManager.Register(new VulnerabilityScanService(new AppNotificationService())); // TODO
            _backgroundServiceManager.Register(new ChecksumVerificationService(new AppNotificationService()));  // TODO
            await _backgroundServiceManager.StartAsync();

            //InitDb(); // TODO
        }

        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.LogError(e.Exception, nameof(Dispatcher_UnhandledException));
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.LogError(e.ExceptionObject as Exception, nameof(CurrentDomain_UnhandledException));
        }

        private static ILogger CreateLogger()
        {
            var factory = new NLogLoggerFactory();
            return factory.CreateLogger("");
        }
    }

    public sealed class LaunchOption
    {
        [Option("background", Required = false)]
        public bool IsBackground { get; set; }
    }
}
