using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace Osmy.ViewModels
{
    internal class AddSbomDialogViewModel : BindableBase, IDialogAware
    {
        public string Title => "Add SBOM";

        public ReactiveProperty<string> Name { get; }
        public ReactiveProperty<string> SbomFileName { get; }
        public ReactiveProperty<string> LocalDirectoryPath { get; }

        public ReactiveCommand<string> CloseDialogCommand { get; }

        public event Action<IDialogResult>? RequestClose;

        public AddSbomDialogViewModel()
        {
            Name = new ReactiveProperty<string>()
                .SetValidateNotifyError(value => string.IsNullOrWhiteSpace(value) ? "Name cannot be empty." : null);
            SbomFileName = new ReactiveProperty<string>()
                .SetValidateNotifyError(value => string.IsNullOrWhiteSpace(value) ? "Select a SBOM file." : null);
            LocalDirectoryPath = new ReactiveProperty<string>();

            CloseDialogCommand = Name.ObserveHasErrors
                .Select(x => !x)
                .ToReactiveCommand<string>()
                .WithSubscribe(param => CloseDialog(param));
        }

        public bool CanCloseDialog()
        {
            return true;    // MEMO: ここでfalseを返すとxで終了もできなくなるようなので常にtrueを返す
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {

        }

        public void CloseDialog(string parameter)
        {
            ButtonResult result = ButtonResult.None;
            DialogParameters parameters = new() {
                { "name", Name.Value },
                { "sbom", SbomFileName.Value },
                { "localDirectory", LocalDirectoryPath.Value }
            };

            if (parameter.Equals("ok", StringComparison.OrdinalIgnoreCase))
            {
                result = ButtonResult.OK;
            }

            RaiseRequestClose(new DialogResult(result, parameters));
        }

        private void RaiseRequestClose(IDialogResult result)
        {
            RequestClose?.Invoke(result);
        }
    }
}
