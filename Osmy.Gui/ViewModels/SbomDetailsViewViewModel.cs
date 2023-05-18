using Osmy.Api;
using Osmy.Core.Data.Sbom;
using Reactive.Bindings;
using System.Collections.Generic;
using System.Linq;

namespace Osmy.Gui.ViewModels
{
    public class SbomDetailsViewViewModel : ViewModelBase
    {
        /// <summary>
        /// ソフトウェア
        /// </summary>
        public ReactivePropertySlim<Sbom> Sbom { get; }

        public ReactivePropertySlim<Sbom[]> RelatedSboms { get; set; }

        public DelegateCommand PathSelectedCommand => _pathSelectedCommand ??= new DelegateCommand(OnPathSelected);
        private DelegateCommand? _pathSelectedCommand;

        //public DelegateCommand<string> CopyChecksumToClipboardCommand => _copyChecksumToClipboardCommand ??= new DelegateCommand<string>(CopyToClipboard);
        //private DelegateCommand<string>? _copyChecksumToClipboardCommand;

        public SbomDetailsViewViewModel(Sbom sbom)
        {
            Sbom = new ReactivePropertySlim<Sbom>(sbom);

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

            RelatedSboms = new ReactivePropertySlim<Sbom[]>(FetchRelatedSboms().ToArray());
        }

        //private void OnSoftwareVulnerabilityScanned()
        //{
        //    ScanResult.Value = FetchLatestScanResult();
        //}

        private IEnumerable<Sbom> FetchRelatedSboms()
        {
            using var client = new RestClient();
            return client.GetRelatedSboms(Sbom.Value.Id);
        }

        private async void OnPathSelected()
        {
            using var client = new RestClient();
            await client.UpdateSbomAsync(Sbom.Value.Id, new UpdateSbomInfo(null, Sbom.Value.LocalDirectory));

            if (Sbom.Value.LocalDirectory is null)
            {
                return;
            }

            // TODO チェックサム検証結果の更新確認リストに登録
        }

        //private void CopyToClipboard(string value)
        //{
        //    Clipboard.SetText(value);
        //}
    }
}
