using Microsoft.EntityFrameworkCore;
using Osmy.Models;
using Osmy.Models.Sbom;
using Prism.Mvvm;
using Reactive.Bindings;
using System.IO;
using System.Linq;

namespace Osmy.ViewModels
{
    public class DashboardViewViewModel : BindableBase
    {
        public ReactivePropertySlim<Sbom[]> VulnerableSoftwares { get; }
        public ReactivePropertySlim<string[]> InvalidHashes { get; set; }

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

            var invalidHashes = dbContext.HashValidationResults
                .Include(x => x.SbomFile)
                .ThenInclude(x => x.Sbom)
                .GroupBy(x => x.SbomFile)
                .AsEnumerable()
                .Select(g => g.MaxBy(x => x.Executed)!)
                .Where(x => x.Result != HashValidationResult.Valid)
                .Select(x => Path.GetFullPath(Path.Join(x.SbomFile.Sbom.LocalDirectory, x.SbomFile.FileName)))
                .ToArray();
            InvalidHashes = new ReactivePropertySlim<string[]>(invalidHashes);
        }


    }
}
