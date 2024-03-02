using Osmy.Api;
using Osmy.Core.Data.Sbom;
using Osmy.Core.Util;
using Osmy.Gui.Util;
using Osmy.Gui.Views;
using Reactive.Bindings;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Osmy.Gui.ViewModels
{
    public class AddSbomDialogViewModel : ViewModelBase
    {
        public static string Title => "Add Software";

        public ReactiveProperty<string> Name { get; }
        public ReactiveProperty<string> SbomFileName { get; }
        public ReactiveProperty<string?> LocalDirectory { get; }

        public AsyncReactiveCommand<ICloseable<Sbom>> CloseDialogCommand { get; }

        public AddSbomDialogViewModel()
        {
            Name = new ReactiveProperty<string>()
                .SetValidateNotifyError(value => string.IsNullOrWhiteSpace(value) ? "Name cannot be empty." : null);
            SbomFileName = new ReactiveProperty<string>()
                .SetValidateNotifyError(value => string.IsNullOrWhiteSpace(value) || !SpdxUtil.HasValidExtension(value) ? "Select an SPDX file." : null);
            LocalDirectory = new ReactiveProperty<string?>();

            var canCloseDialog = Observable.CombineLatest(Name.ObserveHasErrors, SbomFileName.ObserveHasErrors, LocalDirectory.ObserveHasErrors)
                .Select(x => x.All(hasErrors => !hasErrors));
            CloseDialogCommand = new AsyncReactiveCommand<ICloseable<Sbom>>(canCloseDialog).WithSubscribe(CloseDialog);
        }

        public async Task CloseDialog(ICloseable<Sbom> closable)
        {
            using var client = new RestClient();
            var sbomInfo = new AddSbomInfo(Name.Value, SbomFileName.Value, LocalDirectory.Value);
            
            var start = DateTime.Now;
            var sbom = await client.CreateSbomAsync(sbomInfo);

            if (sbom is null)
            {
                await MessageBoxUtil.ShowErrorDialogAsync($"Failed to add software \"{sbomInfo.Name}\".");
                return;
            }

            closable.CloseWithResult(sbom);
        }
    }

    public record SelectedSbomInfo(string Name, string SbomFileName, string? LocalDirectory);
}
