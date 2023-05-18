using Avalonia;
using Avalonia.Controls;
using Osmy.Core.Data.Sbom;

namespace Osmy.Gui.Controls
{
    public partial class SbomInfoView : UserControl
    {
        public SbomInfoView()
        {
            InitializeComponent();
        }

        public Sbom? Value
        {
            get { return _value; }
            set { SetAndRaise(ValueProperty, ref _value, value); }
        }
        private Sbom? _value;
        public static readonly DirectProperty<SbomInfoView, Sbom?> ValueProperty =
            AvaloniaProperty.RegisterDirect<SbomInfoView, Sbom?>(nameof(Value), o => o._value, (o, value) => o.Value = value);
    }
}
