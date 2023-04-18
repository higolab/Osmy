using CycloneDX.Spdx.Models.v2_2;
using Microsoft.EntityFrameworkCore;

namespace Osmy.Server.Data.Sbom
{
    public sealed class SbomFile
    {
        public int Id { get; set; }

        public Sbom Sbom { get; set; }
        public int SbomId { get; set; }

        public string FileName { get; set; }

        [DeleteBehavior(DeleteBehavior.Cascade)]
        public List<Core.Data.Sbom.SbomFileChecksum> Checksums { get; set; }

        public SbomFile()
        {
            Sbom = default!;
            FileName = string.Empty;
            Checksums = new List<Core.Data.Sbom.SbomFileChecksum>();
        }

        public SbomFile(Sbom sbom, string fileName, IEnumerable<Core.Data.Sbom.SbomFileChecksum> checksums)
        {
            Sbom = sbom;
            FileName = fileName;
            Checksums = checksums.ToList();
        }
    }

    public static class ChecksumExtensions
    {
        public static Core.Data.Sbom.SbomFileChecksum ToSbomFileChecksum(this Checksum checksum)
        {
            var algorithm = checksum.Algorithm switch
            {
                ChecksumAlgorithm.SHA256 => Core.Data.Sbom.ChecksumAlgorithm.SHA256,
                ChecksumAlgorithm.SHA1 => Core.Data.Sbom.ChecksumAlgorithm.SHA1,
                ChecksumAlgorithm.SHA384 => Core.Data.Sbom.ChecksumAlgorithm.SHA384,
                ChecksumAlgorithm.MD2 => Core.Data.Sbom.ChecksumAlgorithm.MD2,
                ChecksumAlgorithm.MD4 => Core.Data.Sbom.ChecksumAlgorithm.MD4,
                ChecksumAlgorithm.SHA512 => Core.Data.Sbom.ChecksumAlgorithm.SHA512,
                ChecksumAlgorithm.MD6 => Core.Data.Sbom.ChecksumAlgorithm.MD6,
                ChecksumAlgorithm.MD5 => Core.Data.Sbom.ChecksumAlgorithm.MD5,
                ChecksumAlgorithm.SHA224 => Core.Data.Sbom.ChecksumAlgorithm.SHA224,
                _ => throw new NotSupportedException()
            };

            return new Core.Data.Sbom.SbomFileChecksum(algorithm, checksum.ChecksumValue);
        }
    }
}
