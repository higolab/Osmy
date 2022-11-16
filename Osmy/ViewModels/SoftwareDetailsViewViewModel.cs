using Osmy.Models;
using Osmy.Models.Sbom;
using OSV.Client.Models;
using Prism.Mvvm;
using QuickGraph;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osmy.ViewModels
{
    public class SoftwareDetailsViewViewModel : BindableBase
    {
        /// <summary>
        /// ソフトウェア
        /// </summary>
        public ReactivePropertySlim<Software> Software { get; set; }

        public ReactivePropertySlim<List<PackageScanResult>> ScanResults { get; set; }

        private readonly ManagedSoftwareContext _dbContext;

        public SoftwareDetailsViewViewModel(Software software)
        {
            // TODO 変更の監視が大変なのでSoftwareは変更通知でなくてよいかも
            Software = new ReactivePropertySlim<Software>(software);
            Software.Value.VulnerabilityScanned += OnSoftwareVulnerabilityScanned;  // TODO 購読解除

            ScanResults = new ReactivePropertySlim<List<PackageScanResult>>();

            _dbContext = new ManagedSoftwareContext();
            FetchLatestScanResult();
        }

        private void OnSoftwareVulnerabilityScanned()
        {
            FetchLatestScanResult();
        }

        private void FetchLatestScanResult()
        {
            if (Software is null || !_dbContext.ScanResults.Any()) { return; }

            ScanResults.Value = _dbContext.ScanResults.FirstOrDefault(r => r.SoftwareId == Software.Value.Id)?.Results ?? new List<PackageScanResult>();
        }
    }
}
