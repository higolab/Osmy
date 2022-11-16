using Osmy.Models;
using Osmy.Models.Sbom;
using Osmy.Views;
using OSV.Client.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using QuickGraph;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Osmy.ViewModels
{
    internal class SoftwareListViewViewModel : BindableBase
    {
        private readonly IDialogService _dialogService;

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>自動遅延読み込みにはオブジェクト作成時のDbContextオブジェクトが必要なので破棄せずに保持する．</remarks>
        private readonly ManagedSoftwareContext _dbContext;

        public ReactivePropertySlim<ObservableCollection<Software>> Softwares { get; }

        public ReactivePropertySlim<Software> SelectedSoftware { get; }

        public ReactivePropertySlim<DependencyGraph> Graph { get; } = new();

        public DelegateCommand AddSoftwareCommand => _addSoftwareCommand ??= new DelegateCommand(OpenSoftwareAddDiaglog);
        private DelegateCommand? _addSoftwareCommand;

        public DelegateCommand ScanVulnsCommand => _scanVulnsCommand ??= new DelegateCommand(ScanVulns);
        private DelegateCommand? _scanVulnsCommand;

        public SoftwareListViewViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            _dbContext = new ManagedSoftwareContext();

            Softwares = new ReactivePropertySlim<ObservableCollection<Software>>(new ObservableCollection<Software>(_dbContext.Softwares));
            SelectedSoftware = new ReactivePropertySlim<Software>();
        }

        private void OpenSoftwareAddDiaglog()
        {
            _dialogService.ShowDialog("AddSoftwareDialog", r =>
            {
                if (r.Result != ButtonResult.OK) { return; }
                var softwareName = r.Parameters.GetValue<string>("name");
                var sbomFile = r.Parameters.GetValue<string>("sbom");
                var software = new Software(softwareName, sbomFile);

                _dbContext.Softwares.Add(software);
                _dbContext.SaveChanges();
                Softwares.Value.Add(software);
            });
        }

        private async void ScanVulns()
        {
            if (SelectedSoftware.Value is null) { return; }

            var sbom = SelectedSoftware.Value.LatestSbom;
            if (sbom is null) { return; }
            Graph.Value = sbom.DependencyGraph;

            var scanner = new VulnerabilityScanner();
            var result = await Task.Run(() => scanner.Scan(sbom)).ConfigureAwait(false);
            
            // TODO スキャン結果を保存
            _dbContext.ScanResults.Add(result);
            _dbContext.SaveChanges();

            SelectedSoftware.Value.RaiseVulnerabilityScanned();
        }
    }
}
