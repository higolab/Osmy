using Osmy.Service.Data.ChecksumVerification;
using Target = Osmy.Core.Data.Sbom;

namespace Osmy.Service.Data.Sbom
{
    public static class SbomDataConverter
    {
        public static Target.VulnerabilityScanResult ConvertVulnerabilityScanResult(VulnerabilityScanResult from)
        {
            return new Target.VulnerabilityScanResult(from.Id, from.SbomId, from.Executed, ConvertPackageScanResults(from.Results));
        }

        public static Target.Sbom ConvertSbom(Sbom from)
        {
            //return new Target.Sbom();
            return new Target.Sbom(
                from.Id,
                from.Name,
                from.LocalDirectory,
                from.Content,
                ConvertSbomFiles(from.Files),
                ConvertExternalReferences(from.ExternalReferences),
                ConvertSbomPackages(from.Packages)
                );
        }

        public static Target.PackageScanResult ConvertPackageScanResult(PackageScanResult from)
        {
            return new Target.PackageScanResult(from.Id, ConvertSbomPackage(from.Package), from.IsVulnerable, from.ResultJson);
        }

        public static IEnumerable<Target.PackageScanResult> ConvertPackageScanResults(IEnumerable<PackageScanResult> from)
        {
            foreach (PackageScanResult result in from)
            {
                yield return ConvertPackageScanResult(result);
            }
        }

        public static Target.SbomPackage ConvertSbomPackage(SbomPackage from)
        {
            return new Target.SbomPackage(from.Id, from.Name, from.Version, from.IsRootPackage);
        }

        public static IEnumerable<Target.SbomPackage> ConvertSbomPackages(IEnumerable<SbomPackage> from)
        {
            foreach (SbomPackage result in from)
            {
                yield return ConvertSbomPackage(result);
            }
        }

        public static Target.ChecksumVerification.ChecksumVerificationResultCollection ConvertChecksumVerificationResultCollection(ChecksumVerificationResultCollection from)
        {
            return new Target.ChecksumVerification.ChecksumVerificationResultCollection(from.Id, from.Executed, from.SbomId, ConvertChecksumVerificationResults(from.Results));
        }

        public static IEnumerable<Target.ChecksumVerification.ChecksumVerificationResult> ConvertChecksumVerificationResults(IEnumerable<ChecksumVerificationResult> from)
        {
            foreach (ChecksumVerificationResult result in from)
            {
                yield return ConvertChecksumVerificationResult(result);
            }
        }

        public static Target.ChecksumVerification.ChecksumVerificationResult ConvertChecksumVerificationResult(ChecksumVerificationResult from)
        {
            return new Target.ChecksumVerification.ChecksumVerificationResult(ConvertSbomFile(from.SbomFile), from.Result);
        }

        public static Target.SbomFile ConvertSbomFile(SbomFile from)
        {
            return new Target.SbomFile(from.Id, from.SbomId, from.FileName, from.Checksums);
        }

        public static IEnumerable<Target.SbomFile> ConvertSbomFiles(IEnumerable<SbomFile> from)
        {
            foreach (SbomFile result in from)
            {
                yield return ConvertSbomFile(result);
            }
        }

        public static Target.ExternalReference ConvertExternalReference(SbomExternalReference from)
        {
            return new Target.ExternalReference(from.Id);
        }

        public static IEnumerable<Target.ExternalReference> ConvertExternalReferences(IEnumerable<SbomExternalReference> from)
        {
            foreach (SbomExternalReference result in from)
            {
                yield return ConvertExternalReference(result);
            }
        }
    }
}
