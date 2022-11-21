using Microsoft.EntityFrameworkCore;
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
    public class SoftwareDetailsViewViewModel : BindableBase
    {
        private readonly IDialogService _dialogService;

        /// <summary>
        /// ソフトウェア
        /// </summary>
        public ReactivePropertySlim<Software> Software { get; }

        public ReactivePropertySlim<List<PackageScanResult>> ScanResults { get; }

        public DelegateCommand AddSbomCommand { get; }

        public SoftwareDetailsViewViewModel(Software software, IDialogService dialogService)
        {
            _dialogService = dialogService;

            Software = new ReactivePropertySlim<Software>(software);
            //Software.Value.VulnerabilityScanned += OnSoftwareVulnerabilityScanned;  // TODO Softwareの値変更に対応

            ScanResults = new ReactivePropertySlim<List<PackageScanResult>>();

            FetchLatestScanResult();

            AddSbomCommand = new DelegateCommand(AddSbom);
        }

        private void OnSoftwareVulnerabilityScanned()
        {
            FetchLatestScanResult();
        }

        private void FetchLatestScanResult()
        {
            using var dbContext = new ManagedSoftwareContext();
            if (Software is null || !dbContext.ScanResults.Any()) { return; }

            ScanResults.Value = dbContext.ScanResults.FirstOrDefault(r => r.SoftwareId == Software.Value.Id)?.Results ?? new List<PackageScanResult>();
        }

        private void AddSbom()
        {
            var parameters = new DialogParameters
            {
                { "software", Software.Value }
            };

            _dialogService.ShowDialog("AddSbomDialog", parameters, r =>
            {
                if (r.Result != ButtonResult.OK) { return; }
                var sbomFile = r.Parameters.GetValue<string>("sbom");

                using var dbContext = new ManagedSoftwareContext();
                var software = dbContext.Softwares.Where(x => x.Id == Software.Value.Id).Include(x => x.Sboms).First();
                var sbom = new Spdx(software, sbomFile);
                software.Sboms.Add(sbom);
                dbContext.SaveChanges();

                Software.Value = software;
            });
        }
    }
}
