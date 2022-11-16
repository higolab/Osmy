using Osmy.Models.Sbom;
using Osmy.ViewModels;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Osmy.Views
{
    /// <summary>
    /// SoftwareDetailsView.xaml の相互作用ロジック
    /// </summary>
    public partial class SoftwareDetailsView : UserControl
    {
        public Software Software
        {
            get { return (Software)GetValue(SoftwareProperty); }
            set { SetValue(SoftwareProperty, value); }
        }
        public static readonly DependencyProperty SoftwareProperty =
            DependencyProperty.Register("Software", typeof(Software), typeof(SoftwareDetailsView), new PropertyMetadata(default, OnSoftwareChanged));

        public ReactivePropertySlim<SoftwareDetailsViewViewModel> ViewModel { get; } = new();

        public SoftwareDetailsView()
        {
            InitializeComponent();
        }

        private static void OnSoftwareChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SoftwareDetailsView view && view.Software is not null)
            {
                view.ViewModel.Value = new SoftwareDetailsViewViewModel(view.Software);
            }
        }
    }
}
