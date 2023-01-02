using Microsoft.EntityFrameworkCore;
using Osmy.Extensions;
using Osmy.Models;
using Osmy.Models.HashValidation;
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
        public ReactivePropertySlim<HashValidationResult[]> HashValidationResults { get; }

        public DelegateCommand PathSelectedCommand => _pathSelectedCommand ??= new DelegateCommand(OnPathSelected);
        private DelegateCommand? _pathSelectedCommand;

        public SbomDetailsViewViewModel(Sbom sbom, IDialogService dialogService)
        {
            _dialogService = dialogService;

            Sbom = new ReactivePropertySlim<Sbom>(sbom);
            HashValidationResults = new ReactivePropertySlim<HashValidationResult[]>(FetchFileHashValidationResult());

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

        private HashValidationResult[] FetchFileHashValidationResult()
        {
            using var dbContext = new ManagedSoftwareContext();
            return dbContext.HashValidationResults
                .Where(x => x.SbomId == Sbom.Value.Id)
                .OrderByDescending(x => x.Executed)
                .Include(x => x.Results)
                .ThenInclude(x => x.SbomFile)
                .ThenInclude(x => x.Checksums)
                .FirstOrDefault()
                ?.Results
                ?.Select(x =>
                {
                    x.SbomFile.Checksums = x.SbomFile.Checksums.OrderBy(checksum => checksum.Algorithm).ToList();
                    return x;
                })
                ?.ToArray() ?? Array.Empty<HashValidationResult>();
        }

        private async void OnPathSelected()
        {
            using var dbContext = new ManagedSoftwareContext();
            var sbom = dbContext.Sboms.First(x => x.Id == Sbom.Value.Id);
            sbom.LocalDirectory = Sbom.Value.LocalDirectory;
            await dbContext.SaveChangesAsync();
        }
    }
}
