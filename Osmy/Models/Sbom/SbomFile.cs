using CycloneDX.Spdx.Models.v2_2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osmy.Models.Sbom
{
    public sealed class SbomFile
    {
        public int Id { get; set; }

        public Sbom Sbom { get; set; }

        public string FileName { get; set; }

        public List<SbomFileChecksum> Checksums { get; set; }

        public SbomFile()
        {
            Sbom = default!;
            FileName = default!;
            Checksums = default!;
        }

        public SbomFile(Sbom sbom, string fileName, IEnumerable<SbomFileChecksum> checksums)
        {
            Sbom = sbom;
            FileName = fileName;
            Checksums = checksums.ToList();
        }
    }

    public class SbomFileChecksum
    {
        public int Id { get; set; }
        public ChecksumAlgorithm Algorithm { get; set; }
        public string Value { get; set; }

        public SbomFileChecksum()
        {
            Value = default!;
        }

        public SbomFileChecksum(ChecksumAlgorithm algorithm, string value)
        {
            Algorithm = algorithm;
            Value = value;
        }
    }

    public static class ChecksumExtensions
    {
        public static SbomFileChecksum ToSbomFileChecksum(this Checksum checksum)
        {
            var algorithm = checksum.Algorithm switch
            {
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.SHA256 => ChecksumAlgorithm.SHA256,
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.SHA1 => ChecksumAlgorithm.SHA1,
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.SHA384 => ChecksumAlgorithm.SHA384,
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.MD2 => ChecksumAlgorithm.MD2,
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.MD4 => ChecksumAlgorithm.MD4,
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.SHA512 => ChecksumAlgorithm.SHA512,
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.MD6 => ChecksumAlgorithm.MD6,
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.MD5 => ChecksumAlgorithm.MD5,
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.SHA224 => ChecksumAlgorithm.SHA224,
                _ => throw new NotSupportedException()
            };

            return new SbomFileChecksum(algorithm, checksum.ChecksumValue);
        }
    }


    public enum ChecksumAlgorithm
    {
        SHA1,
        SHA224,
        SHA256,
        SHA384,
        SHA512,
        SHA3_256,
        SHA3_384,
        SHA3_512,
        BLAKE2b_256,
        BLAKE2b_384,
        BLAKE2b_512,
        BLAKE3,
        MD2,
        MD4,
        MD5,
        MD6,
        ADLER32,
    }
}
