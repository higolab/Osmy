using DynamicData;
using Osmy.Api;
using Osmy.Core.Data.Sbom;
using Osmy.Gui.Util;
using Reactive.Bindings;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Osmy.Gui.ViewModels
{
    public class SbomListViewViewModel : ViewModelBase, IActivatableViewModel
    {
        //private readonly ILogger _logger;
        public ViewModelActivator Activator { get; }

        public Interaction<AddSbomDialogViewModel, Sbom?> ShowAddSbomDialog { get; } = new();

        public ReactivePropertySlim<ObservableCollection<Sbom>> SbomInfos { get; }

        public ReactivePropertySlim<Sbom?> SelectedSbomInfo { get; }

        public ReadOnlyReactivePropertySlim<SbomDetailsViewViewModel?> SelectedSbomVM { get; }

        public DelegateCommand AddSbomCommand => _addSbomCommand ??= new DelegateCommand(OpenSbomAddDiaglog);
        private DelegateCommand? _addSbomCommand;

        public Reactive.Bindings.ReactiveCommand DeleteSbomCommand { get; }

        private CancellationTokenSource? _cancelledOnDeactivated;

        public SbomListViewViewModel(/*ILogger logger*/)
        {
            //_logger = logger;
            Activator = new ViewModelActivator();
            this.WhenActivated(disposables =>
            {
                OnActivated();
                Disposable.Create(() => OnDeactivated()).DisposeWith(disposables);
            });

            SbomInfos = new ReactivePropertySlim<ObservableCollection<Sbom>>(new ObservableCollection<Sbom>(FetchSbomInfos()));
            SelectedSbomInfo = new ReactivePropertySlim<Sbom?>();
            SelectedSbomVM = SelectedSbomInfo
                .Select(x => x is null ? null : new SbomDetailsViewViewModel(GetSbomFullData(x.Id)))
                .ToReadOnlyReactivePropertySlim();
            DeleteSbomCommand = new Reactive.Bindings.ReactiveCommand(SelectedSbomInfo.Select(x => x is not null), false);
            // This disposable is not explicitly destroyed, but that's okay as it doesn't cause memory leaks
            DeleteSbomCommand.Subscribe(DeleteSbom);
        }

        private async void OpenSbomAddDiaglog()
        {
            var store = new AddSbomDialogViewModel();
            var sbom = await ShowAddSbomDialog.Handle(store);
            if (sbom is null) { return; }
            SbomInfos.Value.Add(sbom);
        }

        private async void DeleteSbom()
        {
            if (SelectedSbomInfo.Value is null) { return; }

            using var client = new RestClient();
            if (await client.DeleteSbomAsync(SelectedSbomInfo.Value.Id))
            {
                SbomInfos.Value.Remove(SelectedSbomInfo.Value);
                SelectedSbomInfo.Value = null;
            }
            else
            {
                await MessageBoxUtil.ShowErrorDialogAsync($"Failed to delete software \"{SelectedSbomInfo.Value.Name}\".");
            }
        }

        private static IEnumerable<Sbom> FetchSbomInfos()
        {
            using var client = new RestClient();
            try
            {
                return client.GetSboms();
            }
            catch
            {
                // TODO show message dialog
                return Enumerable.Empty<Sbom>();
            }
        }

        private static Sbom GetSbomFullData(long sbomId)
        {
            using var client = new RestClient();
            return client.GetSbom(sbomId) ?? throw new InvalidOperationException();
        }

        private async Task UpdateToLatestResult(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var sbomsNeedUpdate = SbomInfos.Value.Where(IsNeedUpdate).ToArray();
                using (var client = new RestClient())
                {
                    foreach (var current in sbomsNeedUpdate)
                    {
                        var updated = await client.GetSbomAsync(current.Id, cancellationToken);
                        if (updated is not null)
                        {
                            SbomInfos.Value.Replace(current, updated);
                        }
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }

            static bool IsNeedUpdate(Sbom sbom)
            {
                return sbom.LastVulnerabilityScan is null
                    || (sbom.LocalDirectory is not null && sbom.LastFileCheck is null);
            }
        }

        private void OnActivated()
        {
            _cancelledOnDeactivated = new();
            _ = UpdateToLatestResult(_cancelledOnDeactivated.Token);
        }

        private void OnDeactivated()
        {
            _cancelledOnDeactivated?.Cancel();
            _cancelledOnDeactivated?.Dispose();
        }
    }
}
