using Microsoft.EntityFrameworkCore;
using Osmy.Models;
using Osmy.Models.Sbom;
using Prism.Mvvm;
using Reactive.Bindings;
using System.Linq;

namespace Osmy.ViewModels
{
    public class DashboardViewViewModel : BindableBase
    {
        public ReactivePropertySlim<Sbom[]> VulnerableSoftwares { get; }
        public ReactivePropertySlim<Sbom[]> FileErrorSoftwares { get; set; }

        public DashboardViewViewModel()
        {
            using var dbContext = new ManagedSoftwareContext();

            var vulnerableSoftwares = dbContext.ScanResults
                .Include(x => x.Sbom)
                .GroupBy(x => x.SbomId)
                .AsEnumerable()
                .Select(g => g.MaxBy(x => x.Executed)!)
                .Where(x => x.IsVulnerable)
                .Distinct()
                .Select(x => x.Sbom)
                .ToArray();
            VulnerableSoftwares = new ReactivePropertySlim<Sbom[]>(vulnerableSoftwares);

            var fileErrorSoftwares = dbContext.ChecksumVerificationResults
                .Include(x => x.Sbom)
                .AsEnumerable()
                .GroupBy(x => x.SbomId)
                .Select(g => g.MaxBy(x => x.Executed)!)
                .Where(x => x.HasError)
                .Select(x => x.Sbom)
                .ToArray();
            FileErrorSoftwares = new ReactivePropertySlim<Sbom[]>(fileErrorSoftwares);
        }


    }
}
