using Osmy.Core.Util;
using Reactive.Bindings;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace Osmy.Gui.ViewModels
{
    public class AddSbomDialogViewModel : ViewModelBase
    {
        public static string Title => "Add Software";

        public ReactiveProperty<string> Name { get; }
        public ReactiveProperty<string> SbomFileName { get; }
        public ReactiveProperty<string?> LocalDirectory { get; }

        public ReactiveUI.ReactiveCommand<string, SelectedSbomInfo> CloseDialogCommand { get; }

        public AddSbomDialogViewModel()
        {
            Name = new ReactiveProperty<string>()
                .SetValidateNotifyError(value => string.IsNullOrWhiteSpace(value) ? "Name cannot be empty." : null);
            SbomFileName = new ReactiveProperty<string>()
                .SetValidateNotifyError(value => string.IsNullOrWhiteSpace(value) || !SpdxUtil.HasValidExtension(value) ? "Select an SPDX file." : null);
            LocalDirectory = new ReactiveProperty<string?>();

            var canCloseDialog = Observable.CombineLatest(Name.ObserveHasErrors, SbomFileName.ObserveHasErrors, LocalDirectory.ObserveHasErrors)
                .Select(x => x.All(hasErrors => !hasErrors));
            CloseDialogCommand = ReactiveUI.ReactiveCommand.Create<string, SelectedSbomInfo>(CloseDialog, canCloseDialog);
        }

        public SelectedSbomInfo CloseDialog(string parameter)
        {
            if (parameter.Equals("ok", StringComparison.OrdinalIgnoreCase))
            {
                return new SelectedSbomInfo(Name.Value, SbomFileName.Value, LocalDirectory.Value);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    public record SelectedSbomInfo(string Name, string SbomFileName, string? LocalDirectory);
}
