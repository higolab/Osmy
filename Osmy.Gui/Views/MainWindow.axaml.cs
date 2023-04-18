using Avalonia.Controls;
using Osmy.Gui.ViewModels;
using ReactiveUI;
using System.Threading.Tasks;
using System;
using Avalonia.ReactiveUI;

namespace Osmy.Gui.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            //this.WhenActivated(d => d(ViewModel!.ShowSettingDialog.RegisterHandler(ShowSettingDialogAsync)));
        }

        //private async Task ShowSettingDialogAsync(InteractionContext<SettingDialogViewModel, bool> interaction)
        //{
        //    var dialog = new SettingDialog
        //    {
        //        DataContext = interaction.Input
        //    };

        //    var topLevel = GetTopLevel(this);
        //    var window = topLevel as Window ?? throw new InvalidOperationException();
        //    var result = await dialog.ShowDialog<bool>(window);
        //    interaction.SetOutput(result);
        //}
    }
}
