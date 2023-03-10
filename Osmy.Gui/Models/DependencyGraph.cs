using Osmy.Gui.Models.Sbom;
using QuikGraph;

namespace Osmy.Gui.Models
{
    public class DependencyGraph : BidirectionalGraph<SbomPackage, IEdge<SbomPackage>>
    {

    }
}
