using System.ComponentModel.DataAnnotations;

namespace Osmy.Gui.Models.Sbom
{
    /// <summary>
    /// パッケージ
    /// </summary>
    public abstract class SbomPackage
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Version { get; set; }
        public bool IsRootPackage { get; set; }

        public SbomPackage() { }

        public SbomPackage(string name, string version, bool isRootPackage)
        {
            Name = name;
            Version = version;
            IsRootPackage = isRootPackage;
        }
    }
}
