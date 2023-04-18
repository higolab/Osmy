using OSV.Client.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Osmy.Core.Data.Sbom
{
    public class PackageScanResult
    {
        public int Id { get; set; }

        public SbomPackage Package { get; set; }

        public bool IsVulnerable { get; set; }

        /// <summary>
        /// 診断結果のJSON文字列
        /// </summary>
        public string ResultJson { get; set; }

        [JsonIgnore]
        public VulnerabilityList VulnerabilityList => _vulnerabilityList ??= (JsonSerializer.Deserialize<VulnerabilityList>(ResultJson) ?? new VulnerabilityList());
        private VulnerabilityList? _vulnerabilityList;

        public PackageScanResult()
        {
            Package = default!;
            ResultJson = string.Empty;
        }

        public PackageScanResult(SbomPackage package, VulnerabilityList result)
            : this(default, package, result?.Vulnerabilities?.Any() ?? false, JsonSerializer.Serialize(result)) { }

        public PackageScanResult(int id, SbomPackage package, bool isVulnerable, string resultJson)
        {
            Id = id;
            Package = package;
            IsVulnerable = isVulnerable;
            ResultJson = resultJson;
        }
    }
}
