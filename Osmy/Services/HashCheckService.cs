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
                        var sha1Hash = file.Checksums.First(x => x.Algorithm == CycloneDX.Spdx.Models.v2_2.ChecksumAlgorithm.SHA1);
                        var localFileHash = await ComputeSHA1Async(path, stoppingToken);

                        if (!sha1Hash.ChecksumValue.Equals(localFileHash, StringComparison.OrdinalIgnoreCase))
                        {
                            // TODO ハッシュの不一致を記録
                        }
                    }
                }
            }
        }

        private static async Task<string> ComputeSHA1Async(string filePath, CancellationToken cancellationToken)
        {
            HashAlgorithm hashAlgorithm = SHA1.Create();

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
