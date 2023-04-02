using Osmy.Api;
using Osmy.Core.Data.Sbom;
using Osmy.Core.Data.Sbom.ChecksumVerification;
using Reactive.Bindings;
using System;
using System.Threading.Tasks;

namespace Osmy.Gui.ViewModels
{
    public class SbomDetailsViewViewModel : ViewModelBase
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

        //public DelegateCommand<string> CopyChecksumToClipboardCommand => _copyChecksumToClipboardCommand ??= new DelegateCommand<string>(CopyToClipboard);
        //private DelegateCommand<string>? _copyChecksumToClipboardCommand;

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

        //private void OnSoftwareVulnerabilityScanned()
        //{
        //    ScanResult.Value = FetchLatestScanResult();
        //}

        private VulnerabilityScanResult? FetchLatestScanResult()
        {
            using var client = new RestClient();
            return client.GetLatestVulnerabilityScanResult(Sbom.Value.Id);
        }

        private ChecksumVerificationResultCollection? FetchChecksumVerificationResult()
        {
            using var client = new RestClient();
            return client.GetLatestChecksumVerificationResultCollection(Sbom.Value.Id);
        }

        private SbomInfo[] FetchRelatedSboms()
        {
            // TODO
            //using var dbContext = new ManagedSoftwareContext();
            //return Sbom.Value switch
            //{
            //    Spdx => dbContext.Sboms
            //    .OfType<Spdx>()
            //    .AsEnumerable()
            //    .Where(sbom => Sbom.Value.ExternalReferences.OfType<SpdxExternalReference>().Any(exref => exref.DocumentNamespace == sbom.DocumentNamespace))
            //    .Select(sbom =>
            //    {
            //        var isVulnerable = dbContext.ScanResults.Where(x => x.SbomId == sbom.Id).AsEnumerable().MaxBy(x => x.Executed)?.IsVulnerable ?? false;
            //        var hasFileError = dbContext.ChecksumVerificationResults.Where(x => x.SbomId == sbom.Id).OrderByDescending(x => x.Executed).FirstOrDefault()?.HasError ?? false;
            //        return new SbomInfo(sbom, isVulnerable, hasFileError);
            //    })
            //    .ToArray(),
            //    _ => throw new NotSupportedException(),
            //};

            return Array.Empty<SbomInfo>();
        }

        private async void OnPathSelected()
        {
            using var client = new RestClient();
            await client.UpdateSbomAsync(Sbom.Value);

            if (Sbom.Value.LocalDirectory is null)
            {
                return;
            }

            // TODO
            //var result = await Task.Run(() => BackgroundServiceManager.Instance.Resolve<ChecksumVerificationService>().Verify(sbom));
            //dbContext.ChecksumVerificationResults.Add(result);
            //await dbContext.SaveChangesAsync();
            //ChecksumVerificationResults.Value = result;
        }

        //private void CopyToClipboard(string value)
        //{
        //    Clipboard.SetText(value);
        //}
    }
}
