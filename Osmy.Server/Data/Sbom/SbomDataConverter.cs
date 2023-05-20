using Target = Osmy.Core.Data.Sbom;

namespace Osmy.Server.Data.Sbom
{
    public static class SbomDataConverter
    {
        public static Target.Sbom ConvertSbom(Sbom from)
        {
            //return new Target.Sbom();
            return new Target.Sbom(
                from.Id,
                from.Name,
                from.LocalDirectory,
                from.LastVulnerabilityScan,
                from.IsVulnerable,
                from.LastFileCheck,
                from.HasFileError,
                ConvertSbomFiles(from.Files),
                ConvertExternalReferences(from.ExternalReferences),
                ConvertSbomPackages(from.Packages)
                );
        }

        public static Target.SbomPackage ConvertSbomPackage(SbomPackageComponent from)
        {
            return new Target.SbomPackage(from.Id,
                                          from.Name,
                                          from.Version,
                                          from.IsRootPackage,
                                          from.IsDependentPackage,
                                          ConvertVulnerabilityDataList(from.Vulnerabilities));
        }

        public static IEnumerable<Target.SbomPackage> ConvertSbomPackages(IEnumerable<SbomPackageComponent> from)
        {
            foreach (SbomPackageComponent result in from)
            {
                yield return ConvertSbomPackage(result);
            }
        }

        public static Target.SbomFile ConvertSbomFile(SbomFileComponent from)
        {
            return new Target.SbomFile(from.Id, from.SbomId, from.FileName, from.Checksums);
        }

        public static IEnumerable<Target.SbomFile> ConvertSbomFiles(IEnumerable<SbomFileComponent> from)
        {
            foreach (SbomFileComponent result in from)
            {
                yield return ConvertSbomFile(result);
            }
        }

        public static Target.ExternalReference ConvertExternalReference(SbomExternalReferenceComponent from)
        {
            return new Target.ExternalReference(from.Id);
        }

        public static IEnumerable<Target.ExternalReference> ConvertExternalReferences(IEnumerable<SbomExternalReferenceComponent> from)
        {
            foreach (SbomExternalReferenceComponent result in from)
            {
                yield return ConvertExternalReference(result);
            }
        }

        public static Target.VulnerabilityData ConvertVulnerabilityData(VulnerabilityData from)
        {
            return new Target.VulnerabilityData(from.Id, from.Modified, from.Data.Data);
        }

        public static IEnumerable<Target.VulnerabilityData> ConvertVulnerabilityDataList(IEnumerable<VulnerabilityData> from)
        {
            foreach (VulnerabilityData vulnerability in from)
            {
                yield return ConvertVulnerabilityData(vulnerability);
            }
        }
    }
}
