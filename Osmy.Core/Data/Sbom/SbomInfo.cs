namespace Osmy.Core.Data.Sbom
{
    public class SbomInfo
    {
        public Sbom Sbom { get; set; }
        public bool? IsVulnerable { get; set; }
        public bool? HasFileError { get; set; }

        public SbomInfo(Sbom sbom, bool? isVulnerable, bool? hasFileError)
        {
            Sbom = sbom;
            IsVulnerable = isVulnerable;
            HasFileError = hasFileError;
        }
    }
}
