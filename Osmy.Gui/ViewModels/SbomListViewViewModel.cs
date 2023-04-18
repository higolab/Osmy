using Osmy.Api;
using Osmy.Core.Data.Sbom;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Osmy.Gui.ViewModels
{
    public class SbomListViewViewModel : ViewModelBase
    {
        //private readonly IDialogService _dialogService;
        //private readonly IMessageBoxService _messageBoxService;
        //private readonly ILogger _logger;

        public ReactiveUI.Interaction<AddSbomDialogViewModel, SelectedSbomInfo?> ShowAddSbomDialog { get; } = new();

        public ReactivePropertySlim<ObservableCollection<SbomInfo>> SbomInfos { get; }

        public ReactivePropertySlim<SbomInfo?> SelectedSbomInfo { get; }

        public ReadOnlyReactivePropertySlim<SbomDetailsViewViewModel?> SelectedSbomVM { get; }

        public DelegateCommand AddSbomCommand => _addSbomCommand ??= new DelegateCommand(OpenSbomAddDiaglog);
        private DelegateCommand? _addSbomCommand;

        public ReactiveCommand DeleteSbomCommand { get; }

        //public DelegateCommand ScanVulnsCommand => _scanVulnsCommand ??= new DelegateCommand(ScanVulns);
        //private DelegateCommand? _scanVulnsCommand;

        public SbomListViewViewModel(/*ILogger logger*/)
        {
            //_dialogService = dialogService;
            //_messageBoxService = messageBoxService;
            //_logger = logger;

            SbomInfos = new ReactivePropertySlim<ObservableCollection<SbomInfo>>(new ObservableCollection<SbomInfo>(FetchSbomInfos()));
            //SbomInfos = new ReactivePropertySlim<ObservableCollection<SbomInfo>>(new ObservableCollection<SbomInfo> { new SbomInfo(new Spdx() { Name = "Test" }, true, true) });
            SelectedSbomInfo = new ReactivePropertySlim<SbomInfo?>();
            SelectedSbomVM = SelectedSbomInfo
                .Select(x => x is null ? null : new SbomDetailsViewViewModel(x.Sbom))
                .ToReadOnlyReactivePropertySlim();
            DeleteSbomCommand = new ReactiveCommand(SelectedSbomInfo.Select(x => x is not null), false)
                .WithSubscribe(DeleteSbom, out var disposable);  // TODO disposableの適切なタイミングでの破棄
        }

        private async void OpenSbomAddDiaglog()
        {
            var store = new AddSbomDialogViewModel();
            var result = await ShowAddSbomDialog.Handle(store);
            if (result is null) { return; }

            //throw new NotImplementedException();
            //Sbom sbom;
            try
            {
                // TODO
                //sbom = new Spdx(result.Name, result.SbomFileName, result.LocalDirectory);
            }
            catch (Exception /*ex*/)
            {
                // TODO
                //const string errorMessage = "Failed to load the SBOM file";
                //_logger.LogError(ex, errorMessage);
                //_messageBoxService.ShowInformationMessage(errorMessage);
                return;
            }

            using var client = new RestClient();
            var sbomInfo = new AddSbomInfo(result.Name, result.SbomFileName, result.LocalDirectory);
            var sbom = await client.CreateSbomAsync(sbomInfo);

            // TODO 初回スキャンの結果取得
            //var vulnsScanResult = await Task.Run(() => BackgroundServiceManager.Instance.Resolve<VulnerabilityScanService>().Scan(sbom));
            //dbContext.ScanResults.Add(vulnsScanResult);

            //ChecksumVerificationResultCollection? checksumVerificationResult = null;
            //if (sbom.LocalDirectory is not null)
            //{
            //    checksumVerificationResult = await Task.Run(() => BackgroundServiceManager.Instance.Resolve<ChecksumVerificationService>().Verify(sbom));
            //    dbContext.ChecksumVerificationResults.Add(checksumVerificationResult);
            //}
            //await dbContext.SaveChangesAsync();

            //SbomInfos.Value.Add(new SbomInfo(sbom, vulnsScanResult.IsVulnerable, checksumVerificationResult?.HasError ?? false));
            // TODO sbomInfoのIsVulnerableとHasFileErrorをnullに設定するので，nullのものを定期的にチェックして最新の結果を取得する処理を追加したい
            if (sbom is not null)
            {
                SbomInfos.Value.Add(new SbomInfo(sbom, null, sbom.LocalDirectory is null ? false : null));
            }
        }

        private async void DeleteSbom()
        {
            if (SelectedSbomInfo.Value is null) { return; }

            using var client = new RestClient();
            if (await client.DeleteSbomAsync(SelectedSbomInfo.Value.Sbom.Id))
            {
                SbomInfos.Value.Remove(SelectedSbomInfo.Value);
            }
            else
            {
                // TODO
            }
        }

        //private async void ScanVulns()
        //{
        //    if (SelectedSbomInfo.Value is null) { return; }

        //    var container = ContainerLocator.Container;
        //    var serviceManager = container.Resolve<BackgroundServiceManager>();
        //    var result = await Task.Run(() => serviceManager.Resolve<VulnerabilityScanService>().Scan(SelectedSbomInfo.Value.Sbom, CancellationToken.None)).ConfigureAwait(false);

        //    using var dbContext = new ManagedSoftwareContext();
        //    var tmpSbom = dbContext.Sboms.First(x => x.Id == result.Sbom.Id);
        //    result.Sbom = tmpSbom;
        //    dbContext.ScanResults.Add(result);
        //    await dbContext.SaveChangesAsync().ConfigureAwait(false);
        //}

        private static IEnumerable<SbomInfo> FetchSbomInfos()
        {
            using var client = new RestClient();
            return client.GetSboms();
        }
    }
}
