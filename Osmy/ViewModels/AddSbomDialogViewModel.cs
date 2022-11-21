using Osmy.Models.Sbom;
using Prism.Commands;
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
        #region IDialogAware
        public string Title => $"Add SBOM to {Software.Value?.Name}";

        public event Action<IDialogResult>? RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            var software = parameters.GetValue<Software>("software");
            Software.Value = software;
        }
        #endregion

        public ReactivePropertySlim<Software> Software { get; }
        public ReactiveProperty<string> SbomFilePath { get; }

        public ReactiveCommand<string> CloseDialogCommand { get; }

        public DelegateCommand SelectSbomFileCommand => _selectSbomFileComamnd ??= new DelegateCommand(SelectSbomFile);
        private DelegateCommand? _selectSbomFileComamnd;

        public AddSbomDialogViewModel()
        {
            Software = new ReactivePropertySlim<Software>();
            SbomFilePath = new ReactiveProperty<string>()
                .SetValidateNotifyError(value => string.IsNullOrWhiteSpace(value) ? "Enter a correct value" : null);

            CloseDialogCommand = SbomFilePath.ObserveHasErrors
                .Select(x => !x)
                .ToReactiveCommand<string>()
                .WithSubscribe(param => CloseDialog(param));
        }

        public void CloseDialog(string parameter)
        {
            ButtonResult result = ButtonResult.None;
            DialogParameters parameters = new() {
                { "sbom", SbomFilePath.Value }
            };

            if (parameter.Equals("ok", StringComparison.OrdinalIgnoreCase))
            {
                result = ButtonResult.OK;
            }

            RaiseRequestClose(new DialogResult(result, parameters));
        }

        public async void SelectSbomFile()
        {
            // TODO
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                // 同じファイルは重複登録不可
                var selectedFileHash = await Sbom.ComputeHashAsync(dialog.FileName);
                if (Software.Value.Sboms.Any(x => x.ContentHash.SequenceEqual(selectedFileHash)))
                {
                    // TODO エラーメッセージ
                }
                else
                {
                    SbomFilePath.Value = dialog.FileName;
                }
            }
        }

        private void RaiseRequestClose(IDialogResult result)
        {
            RequestClose?.Invoke(result);
        }
    }
}
