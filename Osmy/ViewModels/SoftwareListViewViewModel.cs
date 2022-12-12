using Microsoft.EntityFrameworkCore;
using Osmy.Models;
using Osmy.Models.Sbom;
using Osmy.Services;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Osmy.ViewModels
{
    internal class SoftwareListViewViewModel : BindableBase
    {
        private readonly IDialogService _dialogService;

        public ReactivePropertySlim<ObservableCollection<Software>> Softwares { get; }

        public ReactivePropertySlim<Software> SelectedSoftware { get; }

        public ReadOnlyReactivePropertySlim<SoftwareDetailsViewViewModel?> SelectedSoftwareVM { get; }

        public DelegateCommand AddSoftwareCommand => _addSoftwareCommand ??= new DelegateCommand(OpenSoftwareAddDiaglog);
        private DelegateCommand? _addSoftwareCommand;

        public DelegateCommand ScanVulnsCommand => _scanVulnsCommand ??= new DelegateCommand(ScanVulns);
        private DelegateCommand? _scanVulnsCommand;

        public SoftwareListViewViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;

            using var dbContext = new ManagedSoftwareContext();
            Softwares = new ReactivePropertySlim<ObservableCollection<Software>>(new ObservableCollection<Software>(dbContext.Softwares.Include(x => x.Sboms)));
            SelectedSoftware = new ReactivePropertySlim<Software>();
            SelectedSoftwareVM = SelectedSoftware.Where(x => x is not null).Select(x => new SoftwareDetailsViewViewModel(x, _dialogService)).ToReadOnlyReactivePropertySlim();
        }

        private void OpenSoftwareAddDiaglog()
        {
            _dialogService.ShowDialog("AddSoftwareDialog", r =>
            {
                if (r.Result != ButtonResult.OK) { return; }
                var softwareName = r.Parameters.GetValue<string>("name");
                var sbomFile = r.Parameters.GetValue<string>("sbom");
                var localDirectory = r.Parameters.GetValue<string>("localDirectory");
                var software = new Software(softwareName, sbomFile) { LocalDirectory = localDirectory };

                using var dbContext = new ManagedSoftwareContext();
                dbContext.Softwares.Add(software);
                dbContext.SaveChanges();
                Softwares.Value.Add(software);
            });
        }

        private async void ScanVulns()
        {
            if (SelectedSoftware.Value is null) { return; }

            var sbom = SelectedSoftware.Value.Sboms.First(x => x.IsUsing);
            if (sbom is null) { return; }

            var container = ContainerLocator.Container;
            var serviceManager = container.Resolve<BackgroundServiceManager>();
            var result = await Task.Run(() => serviceManager.Resolve<VulnerabilityScanService>().Scan(sbom, CancellationToken.None)).ConfigureAwait(false);

            using var dbContext = new ManagedSoftwareContext();
            var software = dbContext.Softwares.First(x => x.Id == result.Software.Id);
            result.Software = software;
            dbContext.ScanResults.Add(result);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            SelectedSoftware.Value.RaiseVulnerabilityScanned();
        }
    }
}
