using Avalonia.ReactiveUI;
using Osmy.Core.Data.Sbom;
using Osmy.Gui.ViewModels;

namespace Osmy.Gui.Views
{
    /// <summary>
    /// AddSbomDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class AddSbomDialog : ReactiveWindow<AddSbomDialogViewModel>, ICloseable<Sbom>
    {
        public AddSbomDialog()
        {
            InitializeComponent();
            //FocusManager.SetFocusedElement(this, sbomName);
        }

        public void CloseWithResult(Sbom result)
        {
            Close(result);
        }
    }
}
