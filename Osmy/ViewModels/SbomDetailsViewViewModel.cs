using Microsoft.EntityFrameworkCore;
using Osmy.Extensions;
using Osmy.Models;
using Osmy.Models.Sbom;
using Osmy.Models.Sbom.Spdx;
using OSV.Client.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
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
    public class SbomDetailsViewViewModel : BindableBase
    {
        private readonly IDialogService _dialogService;

        /// <summary>
        /// ソフトウェア
        /// </summary>
        public ReactivePropertySlim<Sbom> Sbom { get; }

        public ReactivePropertySlim<VulnerabilityScanResult?> ScanResult { get; }
        public ReactivePropertySlim<HashValidation[]> HashValidationResults { get; }

        public SbomDetailsViewViewModel(Sbom sbom, IDialogService dialogService)
        {
            _dialogService = dialogService;

            Sbom = new ReactivePropertySlim<Sbom>(sbom);
            HashValidationResults = new ReactivePropertySlim<HashValidation[]>(FetchFileHashValidationResult());

            //sbom.VulnerabilityScanned += OnSoftwareVulnerabilityScanned;
            //// TODO Disposableの処理
            //Sbom.Subscribe((oldValue, newValue) =>
            //{
            //    if (oldValue is not null)
            //    {
            //        oldValue.VulnerabilityScanned -= OnSoftwareVulnerabilityScanned;
            //    }

            //    if (newValue is not null)
            //    {
            //        newValue.VulnerabilityScanned += OnSoftwareVulnerabilityScanned;
            //        HashValidationResults.Value = FetchFileHashValidationResult();
            //    }
            //});

            ScanResult = new ReactivePropertySlim<VulnerabilityScanResult?>(FetchLatestScanResult());
        }

        private void OnSoftwareVulnerabilityScanned()
        {
            ScanResult.Value = FetchLatestScanResult();
        }

        private VulnerabilityScanResult? FetchLatestScanResult()
        {
            using var dbContext = new ManagedSoftwareContext();
            if (Sbom is null || !dbContext.ScanResults.Any()) { return null; }

            return dbContext.ScanResults
                .Include(x => x.Results).ThenInclude(x => x.Package)
                .Include(x => x.Sbom)
                .Where(x => x.Sbom.Id == Sbom.Value.Id)
                .AsEnumerable()
                .MaxBy(x => x.Executed);
        }

        private HashValidation[] FetchFileHashValidationResult()
        {
            using var dbContext = new ManagedSoftwareContext();
            return dbContext.HashValidationResults
                .Include(x => x.SbomFile)
                .ThenInclude(x => x.Sbom)
                .Where(x => x.SbomFile.Sbom.Id == Sbom.Value.Id)
                .Include(x => x.SbomFile)
                .ThenInclude(x => x.Checksums)
                .GroupBy(x => x.SbomFile.Id)
                .AsEnumerable()
                .Select(g => g.MaxBy(x => x.Executed)!)
                .Select(x =>
                {
                    x.SbomFile.Checksums = x.SbomFile.Checksums.OrderBy(checksum => checksum.Algorithm).ToList();
                    return x;
                })
                .ToArray();
        }
    }
}
