using Microsoft.EntityFrameworkCore;
using Osmy.Models;
using Osmy.Models.HashValidation;
using Osmy.Models.Sbom.Spdx;
using Osmy.Services;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using System.Collections.Generic;
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

        public ReactivePropertySlim<ObservableCollection<SbomInfo>> SbomInfos { get; }

        public ReactivePropertySlim<SbomInfo?> SelectedSbomInfo { get; }

        public ReadOnlyReactivePropertySlim<SbomDetailsViewViewModel?> SelectedSbomVM { get; }

        public DelegateCommand AddSbomCommand => _addSbomCommand ??= new DelegateCommand(OpenSbomAddDiaglog);
        private DelegateCommand? _addSbomCommand;

        public ReactiveCommand DeleteSbomCommand { get; }

        public DelegateCommand ScanVulnsCommand => _scanVulnsCommand ??= new DelegateCommand(ScanVulns);
        private DelegateCommand? _scanVulnsCommand;

        public SbomListViewViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;

            using var dbContext = new ManagedSoftwareContext();

            SbomInfos = new ReactivePropertySlim<ObservableCollection<SbomInfo>>(new ObservableCollection<SbomInfo>(FetchSbomInfos()));
            SelectedSbomInfo = new ReactivePropertySlim<SbomInfo?>();
            SelectedSbomVM = SelectedSbomInfo
                .Select(x => x is null ? null : new SbomDetailsViewViewModel(x.Sbom))
                .ToReadOnlyReactivePropertySlim();
            DeleteSbomCommand = new ReactiveCommand(SelectedSbomInfo.Select(x => x is not null), false)
                .WithSubscribe(DeleteSbom, out var disposable);  // TODO disposableの適切なタイミングでの破棄
        }

        private void OpenSbomAddDiaglog()
        {
            _dialogService.ShowDialog("AddSbomDialog", async r =>
            {
                if (r.Result != ButtonResult.OK) { return; }
                var name = r.Parameters.GetValue<string>("name");
                var sbomFile = r.Parameters.GetValue<string>("sbom");
                var localDirectory = r.Parameters.GetValue<string>("localDirectory");
                var sbom = new Spdx(name, sbomFile, localDirectory);

                using var dbContext = new ManagedSoftwareContext();
                dbContext.Sboms.Add(sbom);
                await dbContext.SaveChangesAsync();

                var container = ContainerLocator.Container;
                var serviceManager = container.Resolve<BackgroundServiceManager>();
                var vulnsScanResult = await Task.Run(() => serviceManager.Resolve<VulnerabilityScanService>().Scan(sbom));
                dbContext.ScanResults.Add(vulnsScanResult);

                HashValidationResultCollection? hashValidationResult = null;
                if (sbom.LocalDirectory is not null)
                {
                    hashValidationResult = await Task.Run(() => serviceManager.Resolve<HashValidationService>().Validate(sbom));
                    dbContext.HashValidationResults.Add(hashValidationResult);
                }
                await dbContext.SaveChangesAsync();

                SbomInfos.Value.Add(new SbomInfo(sbom, vulnsScanResult.IsVulnerable, hashValidationResult?.HasError ?? false));
            });
        }

        private async void DeleteSbom()
        {
            if (SelectedSbomInfo.Value is null) { return; }

            using var dbContext = new ManagedSoftwareContext();
            var sbom = dbContext.Sboms.First(x => x.Id == SelectedSbomInfo.Value.Sbom.Id);
            dbContext.Sboms.Remove(sbom);
            SbomInfos.Value.Remove(SelectedSbomInfo.Value);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        private async void ScanVulns()
        {
            if (SelectedSbomInfo.Value is null) { return; }

            var container = ContainerLocator.Container;
            var serviceManager = container.Resolve<BackgroundServiceManager>();
            var result = await Task.Run(() => serviceManager.Resolve<VulnerabilityScanService>().Scan(SelectedSbomInfo.Value.Sbom, CancellationToken.None)).ConfigureAwait(false);

            using var dbContext = new ManagedSoftwareContext();
            var tmpSbom = dbContext.Sboms.First(x => x.Id == result.Sbom.Id);
            result.Sbom = tmpSbom;
            dbContext.ScanResults.Add(result);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        private IEnumerable<SbomInfo> FetchSbomInfos()
        {
            using var dbContext = new ManagedSoftwareContext();
            foreach (var sbom in dbContext.Sboms.Include(x => x.ExternalReferences))
            {
                var isVulnerable = dbContext.ScanResults.Where(x => x.SbomId == sbom.Id).AsEnumerable().MaxBy(x => x.Executed)?.IsVulnerable ?? false;
                var hasFileError = dbContext.HashValidationResults.Where(x => x.SbomId == sbom.Id).OrderByDescending(x => x.Executed).FirstOrDefault()?.HasError ?? false;
                yield return new SbomInfo(sbom, isVulnerable, hasFileError);
            }
        }
    }
}
