using Microsoft.EntityFrameworkCore;
using Osmy.Models;
using Osmy.ViewModels;
using Osmy.Views;
using Prism.Ioc;
using Prism.Mvvm;
using System.IO;
using System.Windows;

namespace Osmy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<SoftwareListView>();
            containerRegistry.RegisterDialogWindow<MetroDialogWindow>();
            containerRegistry.RegisterDialog<AddSoftwareDialog, AddSoftwareDialogViewModel>();

            ViewModelLocationProvider.Register<SoftwareListView, SoftwareListViewViewModel>();
        }

        private void InitDb()
        {
            var context = new ManagedSoftwareContext();
            var dir = Path.GetDirectoryName(context.DbPath);

            if (dir is not null)
            {
                Directory.CreateDirectory(dir);
            }

            context.Database.Migrate();
        }
    }
}
