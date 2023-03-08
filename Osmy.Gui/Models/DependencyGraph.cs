using Osmy.Gui.Models.Sbom;
using QuickGraph;

namespace Osmy.Gui.Models
{
    public class DependencyGraph : BidirectionalGraph<SbomPackage, IEdge<SbomPackage>>
    {

    }
}
