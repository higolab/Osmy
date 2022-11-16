using System.ComponentModel.DataAnnotations;

namespace Osmy.Models.Sbom
{
    /// <summary>
    /// パッケージ
    /// </summary>
    public abstract class Package
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Version { get; set; }
        public bool IsRootPackage { get; set; }

        public Package() { }

        public Package(string name, string version, bool isRootPackage)
        {
            Name = name;
            Version = version;
            IsRootPackage = isRootPackage;
        }
    }
}
