using Prism.Services.Dialogs;
using System.Windows;


namespace Osmy.Services
{
    internal class MessageBoxService : IMessageBoxService
    {
        private readonly IDialogService _dialogService;

        public MessageBoxService(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public ButtonResult ShowConfirmMessage(string? message, MessageBoxButton messageBoxButton = MessageBoxButton.OKCancel)
        {
            DialogParameters parameters = new()
            {
                { "messageBoxButton", messageBoxButton },
                { "message", message }
            };

            ButtonResult result = default;
            _dialogService.ShowDialog("MessageBoxDialog", parameters, r =>
            {
                result = r.Result;
            });

            return result;
        }

        public ButtonResult ShowInformationMessage(string? message, MessageBoxButton messageBoxButton = MessageBoxButton.OK)
        {
            DialogParameters parameters = new()
            {
                { "messageBoxButton", messageBoxButton },
                { "message", message }
            };

            ButtonResult result = default;
            _dialogService.ShowDialog("MessageBoxDialog", parameters, r =>
            {
                result = r.Result;
            });

            return result;
        }
    }

    internal interface IMessageBoxService
    {
        ButtonResult ShowInformationMessage(string? message, MessageBoxButton messageBoxButton = MessageBoxButton.OK);
        ButtonResult ShowConfirmMessage(string? message, MessageBoxButton messageBoxButton = MessageBoxButton.OKCancel);
    }
}
