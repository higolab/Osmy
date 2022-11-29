using Microsoft.EntityFrameworkCore;
using Osmy.Models;
using Osmy.Models.Sbom;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Osmy.Services
{
    internal class HashCheckService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var context = new ManagedSoftwareContext();
                foreach (Sbom sbom in context.Sboms.Where(x => x.IsUsing).Include(x => x.Software).Where(x => x.Software.LocalDirectory != null).Include(x => x.Files))
                {
                    foreach (SbomFile file in sbom.Files)
                    {
                        var path = Path.Combine(sbom.Software.LocalDirectory!, file.FileName);
                        foreach (var checksum in file.Checksums)
                        {
                            var localFileHash = await ComputeHashAsync(checksum.Algorithm, path, stoppingToken);
                            if (!checksum.ChecksumValue.Equals(localFileHash, StringComparison.OrdinalIgnoreCase))
                            {
                                // TODO ハッシュの不一致を記録
                            }
                        }
                    }
                }
            }
        }

        private static async Task<string> ComputeHashAsync(CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm checksumAlgorithm, string filePath, CancellationToken cancellationToken)
        {
            // TODO 残りのアルゴリズムへの対応
            HashAlgorithm hashAlgorithm = checksumAlgorithm switch
            {
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.SHA256 => SHA256.Create(),
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.SHA1 => SHA1.Create(),
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.SHA384 => SHA384.Create(),
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.MD2 => throw new NotSupportedException(),
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.MD4 => throw new NotSupportedException(),
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.SHA512 => SHA512.Create(),
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.MD6 => throw new NotSupportedException(),
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.MD5 => MD5.Create(),
                CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.SHA224 => throw new NotSupportedException(),
                _ => throw new NotSupportedException()
            };

            using var stream = File.OpenRead(filePath);
            var localFileHash = await hashAlgorithm.ComputeHashAsync(stream, cancellationToken);

            var builder = new StringBuilder();
            foreach (byte data in localFileHash)
            {
                builder.AppendFormat("x2", data);
            }

            return builder.ToString();
        }
    }
}
