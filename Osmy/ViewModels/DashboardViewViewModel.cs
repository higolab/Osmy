using Microsoft.EntityFrameworkCore;
using Osmy.Models;
using Osmy.Models.Sbom;
using OSV.Schema;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osmy.ViewModels
{
    public class DashboardViewViewModel : BindableBase
    {
        public ReactivePropertySlim<int> VulnerableSoftwareNum { get; }
        public ReactivePropertySlim<int> InvalidHashNum { get; }

        public DashboardViewViewModel()
        {
            using var dbContext = new ManagedSoftwareContext();

            var vulnerableSoftwareNum = dbContext.ScanResults
                .GroupBy(x => x.SoftwareId)
                .AsEnumerable()
                .Select(g => g.MaxBy(x => x.Executed)!)
                .Where(x => x.IsVulnerable)
                .Count();
            VulnerableSoftwareNum = new ReactivePropertySlim<int>(vulnerableSoftwareNum);

            var invalidHashNum = dbContext.HashValidationResults
                .GroupBy(x => x.SbomFile)
                .AsEnumerable()
                .Select(g => g.MaxBy(x => x.Executed)!)
                .Where(x => !x.IsValid)
                .Count();
            InvalidHashNum= new ReactivePropertySlim<int>(invalidHashNum);
        }
    }
}
