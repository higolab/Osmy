using Microsoft.EntityFrameworkCore;
using Osmy.Models;
using Osmy.Models.Sbom;
using Osmy.Models.Sbom.Spdx;
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
    internal class SbomListViewViewModel : BindableBase
    {
        private readonly IDialogService _dialogService;

        public ReactivePropertySlim<ObservableCollection<Sbom>> Sboms { get; }

        public ReactivePropertySlim<Sbom> SelectedSbom { get; }

        public ReadOnlyReactivePropertySlim<SbomDetailsViewViewModel?> SelectedSbomVM { get; }

        public DelegateCommand AddSbomCommand => _addSbomCommand ??= new DelegateCommand(OpenSbomAddDiaglog);
        private DelegateCommand? _addSbomCommand;

        public DelegateCommand ScanVulnsCommand => _scanVulnsCommand ??= new DelegateCommand(ScanVulns);
        private DelegateCommand? _scanVulnsCommand;

        public SbomListViewViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;

            using var dbContext = new ManagedSoftwareContext();
            Sboms = new ReactivePropertySlim<ObservableCollection<Sbom>>(new ObservableCollection<Sbom>(dbContext.Sboms));
            SelectedSbom = new ReactivePropertySlim<Sbom>();
            SelectedSbomVM = SelectedSbom.Where(x => x is not null).Select(x => new SbomDetailsViewViewModel(x, _dialogService)).ToReadOnlyReactivePropertySlim();
        }

        private void OpenSbomAddDiaglog()
        {
            _dialogService.ShowDialog("AddSbomDialog", r =>
            {
                if (r.Result != ButtonResult.OK) { return; }
                var name = r.Parameters.GetValue<string>("name");
                var sbomFile = r.Parameters.GetValue<string>("sbom");
                var localDirectory = r.Parameters.GetValue<string>("localDirectory");
                var sbom = new Spdx(name, sbomFile, localDirectory);

                using var dbContext = new ManagedSoftwareContext();
                dbContext.Sboms.Add(sbom);
                dbContext.SaveChanges();
                Sboms.Value.Add(sbom);
            });
        }

        private async void ScanVulns()
        {
            if (SelectedSbom.Value is null) { return; }

            var container = ContainerLocator.Container;
            var serviceManager = container.Resolve<BackgroundServiceManager>();
            var result = await Task.Run(() => serviceManager.Resolve<VulnerabilityScanService>().Scan(SelectedSbom.Value, CancellationToken.None)).ConfigureAwait(false);

            using var dbContext = new ManagedSoftwareContext();
            var tmpSbom = dbContext.Sboms.First(x => x.Id == result.Sbom.Id);
            result.Sbom = tmpSbom;
            dbContext.ScanResults.Add(result);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
