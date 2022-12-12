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
        public ReactivePropertySlim<Software[]> VulnerableSoftwares { get; }
        public ReactivePropertySlim<string[]> InvalidHashes { get; set; }

        public DashboardViewViewModel()
        {
            using var dbContext = new ManagedSoftwareContext();

            var vulnerableSoftwares = dbContext.ScanResults
                .Include(x => x.Software)
                .GroupBy(x => x.SoftwareId)
                .AsEnumerable()
                .Select(g => g.MaxBy(x => x.Executed)!)
                .Where(x => x.IsVulnerable)
                .Select(x => x.Software)
                .ToArray();
            VulnerableSoftwares = new ReactivePropertySlim<Software[]>(vulnerableSoftwares);

            var invalidHashes = dbContext.HashValidationResults
                .Include(x => x.SbomFile)
                .ThenInclude(x => x.Sbom)
                .ThenInclude(x => x.Software)
                .GroupBy(x => x.SbomFile)
                .AsEnumerable()
                .Select(g => g.MaxBy(x => x.Executed)!)
                .Where(x => !x.IsValid)
                .Select(x => Path.GetFullPath(Path.Join(x.SbomFile.Sbom.Software.LocalDirectory, x.SbomFile.FileName)))
                .ToArray();
            InvalidHashes = new ReactivePropertySlim<string[]>(invalidHashes);
        }
    }
}
