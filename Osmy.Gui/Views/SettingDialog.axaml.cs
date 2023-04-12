using Avalonia.ReactiveUI;
using Osmy.Gui.ViewModels;
using ReactiveUI;

namespace Osmy.Gui.Views
{
    public partial class SettingDialog : ReactiveWindow<SettingDialogViewModel>
    {
        public SettingDialog()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.CloseDialogCommand.Subscribe(Close)));
        }
    }
}
