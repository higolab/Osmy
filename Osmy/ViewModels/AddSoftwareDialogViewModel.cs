﻿using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osmy.ViewModels
{
    internal class AddSoftwareDialogViewModel : BindableBase, IDialogAware
    {
        public string Title => "Add Software";

        public ReactiveProperty<string> SoftwareName { get; }
        public ReactivePropertySlim<string> SbomFile { get; }
        public ReactiveProperty<string> LocalDirectoryPath { get; }

        public ReactiveCommand<string> CloseDialogCommand { get; }

        public event Action<IDialogResult>? RequestClose;

        public AddSoftwareDialogViewModel()
        {
            SoftwareName = new ReactiveProperty<string>()
                .SetValidateNotifyError(value => string.IsNullOrWhiteSpace(value) ? "Enter a correct value" : null);
            SbomFile = new ReactivePropertySlim<string>();
            LocalDirectoryPath = new ReactiveProperty<string>();

            CloseDialogCommand = SoftwareName.ObserveHasErrors
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
                { "name", SoftwareName.Value },
                { "sbom", SbomFile.Value },
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
