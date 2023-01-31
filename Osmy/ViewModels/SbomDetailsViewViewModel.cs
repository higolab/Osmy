using Microsoft.EntityFrameworkCore;
using Osmy.Models;
using Osmy.Models.ChecksumVerification;
using Osmy.Models.Sbom;
using Osmy.Models.Sbom.Spdx;
using Osmy.Services;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Osmy.ViewModels
{
    public class SbomDetailsViewViewModel : BindableBase
    {
        /// <summary>
        /// ソフトウェア
        /// </summary>
        public ReactivePropertySlim<Sbom> Sbom { get; }

        public ReactivePropertySlim<VulnerabilityScanResult?> ScanResult { get; }
        public ReactivePropertySlim<ChecksumVerificationResultCollection?> ChecksumVerificationResults { get; }
        public ReactivePropertySlim<SbomInfo[]> RelatedSboms { get; set; }

        public DelegateCommand PathSelectedCommand => _pathSelectedCommand ??= new DelegateCommand(OnPathSelected);
        private DelegateCommand? _pathSelectedCommand;

        public DelegateCommand<string> CopyChecksumToClipboardCommand => _copyChecksumToClipboardCommand ??= new DelegateCommand<string>(CopyToClipboard);
        private DelegateCommand<string>? _copyChecksumToClipboardCommand;

        public SbomDetailsViewViewModel(Sbom sbom)
        {
            Sbom = new ReactivePropertySlim<Sbom>(sbom);
            ChecksumVerificationResults = new ReactivePropertySlim<ChecksumVerificationResultCollection?>(FetchChecksumVerificationResult());

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
            RelatedSboms = new ReactivePropertySlim<SbomInfo[]>(FetchRelatedSboms());
        }

        private void OnSoftwareVulnerabilityScanned()
        {
            ScanResult.Value = FetchLatestScanResult();
        }

        private VulnerabilityScanResult? FetchLatestScanResult()
        {
            using var dbContext = new ManagedSoftwareContext();
            if (Sbom is null || !dbContext.ScanResults.Any()) { return null; }

            var latestScanResultId = dbContext.ScanResults
                .Where(x => x.SbomId == Sbom.Value.Id)
                .OrderByDescending(x => x.Executed)
                .First()
                ?.Id;
            if (latestScanResultId is null) { return null; }

            return dbContext.ScanResults
                .Include(x => x.Results).ThenInclude(x => x.Package)
                .Include(x => x.Sbom)
                .First(x => x.Id == latestScanResultId);
        }

        private ChecksumVerificationResultCollection? FetchChecksumVerificationResult()
        {
            using var dbContext = new ManagedSoftwareContext();
            var resultCollection = dbContext.ChecksumVerificationResults
                .Where(x => x.SbomId == Sbom.Value.Id)
                .OrderByDescending(x => x.Executed)
                .Include(x => x.Results)
                .ThenInclude(x => x.SbomFile)
                .ThenInclude(x => x.Checksums)
                .FirstOrDefault();

            if (resultCollection is not null)
            {
                foreach (var result in resultCollection.Results)
                {
                    result.SbomFile.Checksums = result.SbomFile.Checksums.OrderBy(checksum => checksum.Algorithm).ToList();
                }
            }

            return resultCollection;
        }

        private SbomInfo[] FetchRelatedSboms()
        {
            using var dbContext = new ManagedSoftwareContext();
            return Sbom.Value switch
            {
                Spdx => dbContext.Sboms
                .OfType<Spdx>()
                .AsEnumerable()
                .Where(sbom => Sbom.Value.ExternalReferences.OfType<SpdxExternalReference>().Any(exref => exref.DocumentNamespace == sbom.DocumentNamespace))
                .Select(sbom =>
                {
                    var isVulnerable = dbContext.ScanResults.Where(x => x.SbomId == sbom.Id).AsEnumerable().MaxBy(x => x.Executed)?.IsVulnerable ?? false;
                    var hasFileError = dbContext.ChecksumVerificationResults.Where(x => x.SbomId == sbom.Id).OrderByDescending(x => x.Executed).FirstOrDefault()?.HasError ?? false;
                    return new SbomInfo(sbom, isVulnerable, hasFileError);
                })
                .ToArray(),
                _ => throw new NotSupportedException(),
            };
        }

        private async void OnPathSelected()
        {
            using var dbContext = new ManagedSoftwareContext();
            var sbom = dbContext.Sboms
                .Include(x => x.Files)
                .ThenInclude(x => x.Checksums)
                .First(x => x.Id == Sbom.Value.Id);
            sbom.LocalDirectory = Sbom.Value.LocalDirectory;
            await dbContext.SaveChangesAsync();

            if (sbom.LocalDirectory is null)
            {
                return;
            }

            var container = ContainerLocator.Container;
            var serviceManager = container.Resolve<BackgroundServiceManager>();
            var result = await Task.Run(() => serviceManager.Resolve<ChecksumVerificationService>().Verify(sbom)).ConfigureAwait(false);
            dbContext.ChecksumVerificationResults.Add(result);
            await dbContext.SaveChangesAsync();
            ChecksumVerificationResults.Value = result;
        }

        private void CopyToClipboard(string value)
        {
            Clipboard.SetText(value);
        }
    }
}
