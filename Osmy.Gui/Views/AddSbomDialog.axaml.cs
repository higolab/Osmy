using Avalonia.ReactiveUI;
using Osmy.Gui.ViewModels;
using ReactiveUI;
using System;

namespace Osmy.Gui.Views
{
    /// <summary>
    /// AddSbomDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class AddSbomDialog : ReactiveWindow<AddSbomDialogViewModel>
    {
        public AddSbomDialog()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.CloseDialogCommand.Subscribe(Close)));
            //FocusManager.SetFocusedElement(this, sbomName);
        }
    }
}
