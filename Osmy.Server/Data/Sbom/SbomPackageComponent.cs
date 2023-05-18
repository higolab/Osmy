using Osmy.Server.Services;
using System.ComponentModel.DataAnnotations;

namespace Osmy.Server.Data.Sbom
{
    /// <summary>
    /// パッケージ
    /// </summary>
    public sealed class SbomPackageComponent
    {
        [Key]
        public int Id { get; set; }

        public Sbom Sbom { get; set; }

        public string Name { get; set; }
        
        public string ReferenceId { get; set; }

        public string? Version { get; set; }

        public bool IsRootPackage { get; set; }

        public bool IsDependentPackage { get; set; }

        /// <summary>
        /// 脆弱性リスト
        /// </summary>
        public List<VulnerabilityData> Vulnerabilities { get; set; }

        public SbomPackageComponent() : this(string.Empty, string.Empty, default, default) { }

        public SbomPackageComponent(string name, string referenceId, string? version, bool isRootPackage)
        {
            Sbom = default!;
            Name = name;
            ReferenceId = referenceId;
            Version = version;
            IsRootPackage = isRootPackage;
            IsDependentPackage = isRootPackage; // ルートパッケージであれば依存パッケージであるとする
            Vulnerabilities = new List<VulnerabilityData>();
        }
    }
}
