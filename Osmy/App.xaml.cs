using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Osmy.Models;
using Osmy.Services;
using Osmy.ViewModels;
using Osmy.Views;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Osmy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private readonly BackgroundServiceManager _backgroundServiceManager = new();

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<DashboardView>();
            containerRegistry.RegisterForNavigation<SbomListView>();
            containerRegistry.RegisterDialogWindow<MetroDialogWindow>();
            containerRegistry.RegisterDialog<AddSbomDialog, AddSbomDialogViewModel>();

            ViewModelLocationProvider.Register<DashboardView, DashboardViewViewModel>();
            ViewModelLocationProvider.Register<SbomListView, SbomListViewViewModel>();

            containerRegistry.RegisterInstance(_backgroundServiceManager);
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

            _backgroundServiceManager.Register(new NotifyIconService());
            _backgroundServiceManager.Register(new VulnerabilityScanService());
            _backgroundServiceManager.Register(new HashValidationService());
            await _backgroundServiceManager.StartAsync();

            //InitDb(); // TODO
        }

        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // TODO logging
            //throw new NotImplementedException();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // TODO logging
            //throw new NotImplementedException();
        }
    }
}
