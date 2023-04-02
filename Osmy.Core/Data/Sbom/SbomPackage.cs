using System.ComponentModel.DataAnnotations;

namespace Osmy.Core.Data.Sbom
{
    /// <summary>
    /// パッケージ
    /// </summary>
    public class SbomPackage
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Version { get; set; }
        public bool IsRootPackage { get; set; }

        public SbomPackage() : this(default, string.Empty, default, default) { }

        public SbomPackage(int id, string name, string? version, bool isRootPackage)
        {
            Id = id;
            Name = name;
            Version = version;
            IsRootPackage = isRootPackage;
        }
    }
}
