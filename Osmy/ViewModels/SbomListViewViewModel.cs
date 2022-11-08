using Osmy.Models;
using Osmy.Models.Sbom;
using Osmy.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using QuickGraph;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Osmy.ViewModels
{
    internal class SbomListViewViewModel : BindableBase
    {
        private readonly IDialogService _dialogService;

        public ReactivePropertySlim<ObservableCollection<ISoftware>> Softwares { get; }

        public ReactivePropertySlim<ISoftware> SelectedSouftware { get; }

        public ReactivePropertySlim<DependencyGraph> Graph { get; } = new();

        public DelegateCommand AddSoftwareCommand => _addSoftwareCommand ??= new DelegateCommand(OpenSoftwareAddDiaglog);
        private DelegateCommand? _addSoftwareCommand;

        public DelegateCommand ScanVulnsCommand => _scanVulnsCommand ??= new DelegateCommand(ScanVulns);
        private DelegateCommand? _scanVulnsCommand;

        public SbomListViewViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;

            Softwares = new ReactivePropertySlim<ObservableCollection<ISoftware>>(new ObservableCollection<ISoftware>());
            SelectedSouftware = new ReactivePropertySlim<ISoftware>();
        }

        private void OpenSoftwareAddDiaglog()
        {
            _dialogService.ShowDialog("AddSoftwareDialog", r =>
            {
                if (r.Result != ButtonResult.OK) { return; }
                var softwareName = r.Parameters.GetValue<string>("name");
                var sbomFile = r.Parameters.GetValue<string>("sbom");
                Softwares.Value.Add(new Software(softwareName, sbomFile));
            });
        }

        private async void ScanVulns()
        {
            if (SelectedSouftware.Value is null) { return; }

            var sbom = SelectedSouftware.Value.Sboms.First();   // TODO
            Graph.Value = sbom.DependencyGraph;

            var scanner = new VulnerabilityScanner();
            var result = await Task.Run(() => scanner.Scan(sbom)).ConfigureAwait(false);
        }
    }
}
