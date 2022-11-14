using Osmy.Models;
using Osmy.Models.Sbom;
using Osmy.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Osmy.ViewModels
{
    internal class SoftwareListViewViewModel : BindableBase
    {
        private readonly IDialogService _dialogService;

        private readonly ManagedSoftwareContext _managedSoftwareContext = new();

        public ReactivePropertySlim<ObservableCollection<Software>> Softwares { get; }

        public ReactivePropertySlim<Software> SelectedSouftware { get; }

        public ReactivePropertySlim<DependencyGraph> Graph { get; } = new();

        public DelegateCommand AddSoftwareCommand => _addSoftwareCommand ??= new DelegateCommand(OpenSoftwareAddDiaglog);
        private DelegateCommand? _addSoftwareCommand;

        public DelegateCommand ScanVulnsCommand => _scanVulnsCommand ??= new DelegateCommand(ScanVulns);
        private DelegateCommand? _scanVulnsCommand;

        public SoftwareListViewViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;

            Softwares = new ReactivePropertySlim<ObservableCollection<Software>>(new ObservableCollection<Software>(_managedSoftwareContext.Softwares));
            SelectedSouftware = new ReactivePropertySlim<Software>();
        }

        private void OpenSoftwareAddDiaglog()
        {
            _dialogService.ShowDialog("AddSoftwareDialog", r =>
            {
                if (r.Result != ButtonResult.OK) { return; }
                var softwareName = r.Parameters.GetValue<string>("name");
                var sbomFile = r.Parameters.GetValue<string>("sbom");
                var software = new Software(softwareName, sbomFile);

                _managedSoftwareContext.Softwares.Add(software);
                _managedSoftwareContext.SaveChanges();
                Softwares.Value.Add(software);
            });
        }

        private async void ScanVulns()
        {
            if (SelectedSouftware.Value is null) { return; }

            var sbom = SelectedSouftware.Value.LatestSbom;
            if (sbom is null) { return; }
            Graph.Value = sbom.DependencyGraph;

            var scanner = new VulnerabilityScanner();
            var result = await Task.Run(() => scanner.Scan(sbom)).ConfigureAwait(false);
        }
    }
}
