using Microsoft.EntityFrameworkCore;
using OSV.Client.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Osmy.Service.Data.Sbom
{
    public class PackageScanResult
    {
        [Key]
        public int Id { get; set; }

        [DeleteBehavior(DeleteBehavior.Cascade)]
        public SbomPackage Package { get; set; }

        public bool IsVulnerable { get; set; }

        /// <summary>
        /// 診断結果のJSON文字列
        /// </summary>
        public string ResultJson { get; set; }

        public VulnerabilityList VulnerabilityList => _vulnerabilityList ??= (JsonSerializer.Deserialize<VulnerabilityList>(ResultJson) ?? new VulnerabilityList());
        private VulnerabilityList? _vulnerabilityList;

        public PackageScanResult()
        {
            Package = default!;
            ResultJson = default!;
        }

        public PackageScanResult(SbomPackage package, VulnerabilityList result)
        {
            Package = package;
            IsVulnerable = result?.Vulnerabilities?.Any() ?? false;
            ResultJson = JsonSerializer.Serialize(result); ;
        }
    }
}
