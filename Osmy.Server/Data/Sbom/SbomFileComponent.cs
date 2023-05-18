using CycloneDX.Spdx.Models.v2_2;
using Microsoft.EntityFrameworkCore;
using Osmy.Core.Data.Sbom;

namespace Osmy.Server.Data.Sbom
{
    public sealed class SbomFileComponent
    {
        public int Id { get; set; }

        public Sbom Sbom { get; set; }
        public int SbomId { get; set; }

        public string FileName { get; set; }

        public ChecksumCorrectness Status { get; set; }

        [DeleteBehavior(DeleteBehavior.Cascade)]
        public List<SbomFileChecksum> Checksums { get; set; }

        public SbomFileComponent()
        {
            Sbom = default!;
            FileName = string.Empty;
            Checksums = new List<SbomFileChecksum>();
        }

        public SbomFileComponent(Sbom sbom, string fileName, IEnumerable<SbomFileChecksum> checksums)
        {
            Sbom = sbom;
            FileName = fileName;
            Checksums = checksums.ToList();
        }
    }

    public static class ChecksumExtensions
    {
        public static SbomFileChecksum ToSbomFileChecksum(this Checksum checksum)
        {
            var algorithm = checksum.Algorithm switch
            {
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.SHA256 => Core.Data.Sbom.ChecksumAlgorithm.SHA256,
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.SHA1 => Core.Data.Sbom.ChecksumAlgorithm.SHA1,
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.SHA384 => Core.Data.Sbom.ChecksumAlgorithm.SHA384,
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.MD2 => Core.Data.Sbom.ChecksumAlgorithm.MD2,
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.MD4 => Core.Data.Sbom.ChecksumAlgorithm.MD4,
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.SHA512 => Core.Data.Sbom.ChecksumAlgorithm.SHA512,
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.MD6 => Core.Data.Sbom.ChecksumAlgorithm.MD6,
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.MD5 => Core.Data.Sbom.ChecksumAlgorithm.MD5,
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.SHA224 => Core.Data.Sbom.ChecksumAlgorithm.SHA224,
                _ => throw new NotSupportedException()
            };

            return new SbomFileChecksum(algorithm, checksum.ChecksumValue);
        }
    }
}
