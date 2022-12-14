using GraphSharp.Controls;
using Osmy.Models.Sbom;
using QuickGraph;
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
    /// SoftwareListView.xaml の相互作用ロジック
    /// </summary>
    public partial class SbomListView : UserControl
    {
        public SbomListView()
        {
            InitializeComponent();
        }
    }

    public class DependencyGraph : BidirectionalGraph<SbomPackage, IEdge<SbomPackage>>
    {

    }

    public class DependencyGraphLayout : GraphLayout<SbomPackage, IEdge<SbomPackage>, DependencyGraph>
    {
    }
}
