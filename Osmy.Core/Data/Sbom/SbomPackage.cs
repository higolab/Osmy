namespace Osmy.Core.Data.Sbom
{
    /// <summary>
    /// パッケージ
    /// </summary>
    public class SbomPackage
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? Version { get; set; }
        public bool IsRootPackage { get; set; }
        public bool IsDependentPackage { get; set; }
        public IEnumerable<VulnerabilityData> Vulnerabilities { get; set; }

        public SbomPackage() : this(default, string.Empty, default, default, default, Enumerable.Empty<VulnerabilityData>()) { }

        public SbomPackage(long id,
                           string name,
                           string? version,
                           bool isRootPackage,
                           bool isDependentPackage,
                           IEnumerable<VulnerabilityData> vulnerabilities)
        {
            Id = id;
            Name = name;
            Version = version;
            IsRootPackage = isRootPackage;
            IsDependentPackage = isDependentPackage;
            Vulnerabilities = vulnerabilities;
        }
    }
}
