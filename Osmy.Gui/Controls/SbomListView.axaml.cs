using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Osmy.Core.Data.Sbom;
using Osmy.Gui.ViewModels;
using Osmy.Gui.Views;
using ReactiveUI;
using System;
using System.Threading.Tasks;

namespace Osmy.Gui.Controls
{
    public partial class SbomListView : ReactiveUserControl<SbomListViewViewModel>
    {
        public SbomListView()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.ShowAddSbomDialog.RegisterHandler(ShowAddSbomDialogAsync)));
        }

        private async Task ShowAddSbomDialogAsync(InteractionContext<AddSbomDialogViewModel, Sbom?> interaction)
        {
            var dialog = new AddSbomDialog
            {
                DataContext = interaction.Input
            };

            var topLevel = TopLevel.GetTopLevel(this);
            var window = topLevel as Window ?? throw new InvalidOperationException();
            var result = await dialog.ShowDialog<Sbom?>(window);
            interaction.SetOutput(result);
        }
    }
}
