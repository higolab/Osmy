using Osmy.Models;
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

namespace Osmy.Controls
{
    /// <summary>
    /// SbomInfoPresenter.xaml の相互作用ロジック
    /// </summary>
    public partial class SbomInfoPresenter : UserControl
    {
        public SbomInfoPresenter()
        {
            InitializeComponent();
        }

        public SbomInfo Value
        {
            get { return (SbomInfo)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(SbomInfo), typeof(SbomInfoPresenter), new PropertyMetadata(default));
    }
}
