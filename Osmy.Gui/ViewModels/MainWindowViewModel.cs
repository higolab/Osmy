using System.Reactive.Linq;

namespace Osmy.Gui.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ReactiveUI.Interaction<SettingDialogViewModel, bool> ShowSettingDialog { get; } = new();

        public DelegateCommand OpenSettingDialogCommand => _openSettingDialogCommand ??= new DelegateCommand(OpenSettingDialog);
        private DelegateCommand? _openSettingDialogCommand;

        private async void OpenSettingDialog()
        {
            var store = new SettingDialogViewModel();
            var result = await ShowSettingDialog.Handle(store);
            // TODO
        }
    }
}
