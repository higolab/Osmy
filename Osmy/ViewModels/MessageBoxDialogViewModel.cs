using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using System;
using System.Windows;

namespace Osmy.ViewModels
{
    public class MessageBoxDialogViewModel : BindableBase, IDialogAware
    {
        public string Title { get; set; } = default!;
        public string? Message { get; set; }
        public ButtonVisibility ButtonVisibility { get; set; } = default!;

        public ReactiveCommand<ButtonResult> CloseDialogCommand { get; set; }

        public event Action<IDialogResult>? RequestClose;

        public MessageBoxDialogViewModel()
        {
            CloseDialogCommand = new ReactiveCommand<ButtonResult>().WithSubscribe(CloseDialog);
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<MessageBoxButton>("messageBoxButton", out var buttonType))
            {
                ButtonVisibility = new ButtonVisibility(buttonType);
                RaisePropertyChanged(nameof(ButtonVisibility));

                Title = buttonType switch
                {
                    MessageBoxButton.OK => "Information",
                    MessageBoxButton.OKCancel => "Confirm",
                    MessageBoxButton.YesNoCancel => "Confirm",
                    MessageBoxButton.YesNo => "Confirm",
                    _ => "Information",
                };
            }

            if (parameters.TryGetValue<string?>("message", out var message))
            {
                Message = message;
                RaisePropertyChanged(nameof(Message));
            }
        }

        private void CloseDialog(ButtonResult buttonResult)
        {
            DialogParameters parameters = new();
            RequestClose?.Invoke(new DialogResult(buttonResult, parameters));
        }
    }

    public class ButtonVisibility
    {
        public bool IsOKVisible { get; set; }
        public bool IsYesVisible { get; set; }
        public bool IsNoVisible { get; set; }
        public bool IsCancelVisible { get; set; }

        public ButtonVisibility(MessageBoxButton messageBoxButton)
        {
            switch (messageBoxButton)
            {
                case MessageBoxButton.OK:
                    IsOKVisible = true;
                    break;
                case MessageBoxButton.OKCancel:
                    IsOKVisible = true;
                    IsCancelVisible = true;
                    break;
                case MessageBoxButton.YesNoCancel:
                    IsYesVisible = true;
                    IsNoVisible = true;
                    IsCancelVisible = true;
                    break;
                case MessageBoxButton.YesNo:
                    IsYesVisible = true;
                    IsNoVisible = true;
                    break;
            }
        }
    }
}
