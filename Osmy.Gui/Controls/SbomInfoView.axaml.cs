using Avalonia;
using Avalonia.Controls;
using Osmy.Gui.Models;

namespace Osmy.Gui.Controls
{
    public partial class SbomInfoView : UserControl
    {
        public SbomInfoView()
        {
            InitializeComponent();
        }

        public SbomInfo? Value
        {
            get { return _value; }
            set { SetAndRaise(ValueProperty, ref _value, value); }
        }
        private SbomInfo? _value;
        public static readonly DirectProperty<SbomInfoView, SbomInfo?> ValueProperty =
            AvaloniaProperty.RegisterDirect<SbomInfoView, SbomInfo?>(nameof(Value), o => o._value, (o, value) => o.Value = value);
    }
}
