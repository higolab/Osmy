using Osmy.Models;
using Osmy.Models.Sbom;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace Osmy.ViewModels
{
    internal class AddSbomDialogViewModel : BindableBase, IDialogAware
    {
        public string Title => "Add Sbom";

        public ReactiveProperty<string> Name { get; }
        public ReactivePropertySlim<string> SbomFileName { get; }
        public ReactiveProperty<string> LocalDirectoryPath { get; }

        public ReactiveCommand<string> CloseDialogCommand { get; }

        public event Action<IDialogResult>? RequestClose;

        public AddSbomDialogViewModel()
        {
            Name = new ReactiveProperty<string>()
                .SetValidateNotifyError(value => string.IsNullOrWhiteSpace(value) ? "Enter a correct value" : null);
            SbomFileName = new ReactivePropertySlim<string>();
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

        // TODO 重複ファイルの登録防止
        private async System.Threading.Tasks.Task ValidateSbomFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                // 同じファイルは重複登録不可
                var selectedFileHash = await Sbom.ComputeHashAsync(dialog.FileName);
                using var dbContext = new ManagedSoftwareContext();
                if (dbContext.Sboms.Any(x => x.ContentHash.SequenceEqual(selectedFileHash)))
                {
                    // TODO エラーメッセージ
                }
                else
                {
                    SbomFileName.Value = dialog.FileName;
                }
            }
        }
    }
}
