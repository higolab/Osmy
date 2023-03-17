using Microsoft.EntityFrameworkCore;
using Osmy.Gui.Models;
using Osmy.Gui.Models.ChecksumVerification;
using Osmy.Gui.Models.Sbom;
using Osmy.Gui.Models.Sbom.Spdx;
using Osmy.Gui.Services;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
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

            Sbom sbom;
            try
            {
                sbom = new Spdx(result.Name, result.SbomFileName, result.LocalDirectory);
            }
            catch (Exception /*ex*/)
            {
                // TODO
                //const string errorMessage = "Failed to load the SBOM file";
                //_logger.LogError(ex, errorMessage);
                //_messageBoxService.ShowInformationMessage(errorMessage);
                return;
            }

            using var dbContext = new ManagedSoftwareContext();
            dbContext.Sboms.Add(sbom);
            await dbContext.SaveChangesAsync();

            var vulnsScanResult = await Task.Run(() => BackgroundServiceManager.Instance.Resolve<VulnerabilityScanService>().Scan(sbom));
            dbContext.ScanResults.Add(vulnsScanResult);

            ChecksumVerificationResultCollection? checksumVerificationResult = null;
            if (sbom.LocalDirectory is not null)
            {
                checksumVerificationResult = await Task.Run(() => BackgroundServiceManager.Instance.Resolve<ChecksumVerificationService>().Verify(sbom));
                dbContext.ChecksumVerificationResults.Add(checksumVerificationResult);
            }
            await dbContext.SaveChangesAsync();

            SbomInfos.Value.Add(new SbomInfo(sbom, vulnsScanResult.IsVulnerable, checksumVerificationResult?.HasError ?? false));
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
            using var dbContext = new ManagedSoftwareContext();
            foreach (var sbom in dbContext.Sboms.Include(x => x.ExternalReferences))
            {
                var isVulnerable = dbContext.ScanResults.Where(x => x.SbomId == sbom.Id).AsEnumerable().MaxBy(x => x.Executed)?.IsVulnerable ?? false;
                var hasFileError = dbContext.ChecksumVerificationResults.Where(x => x.SbomId == sbom.Id).OrderByDescending(x => x.Executed).FirstOrDefault()?.HasError ?? false;
                yield return new SbomInfo(sbom, isVulnerable, hasFileError);
            }
        }
    }

    public class DelegateCommand : ICommand
    {
        private readonly Action _action;

        public DelegateCommand(Action action)
        {
            _action = action;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _action.Invoke();
        }
    }

    public class DelegateCommand<T> : ICommand
    {
        private readonly Action<T?> _action;

        public DelegateCommand(Action<T?> action)
        {
            _action = action;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _action.Invoke((T?)parameter);
        }
    }
}
