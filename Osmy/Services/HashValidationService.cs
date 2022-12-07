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
    internal class HashValidationService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // TODO 最近チェックしていないものだけチェックする
                using var context = new ManagedSoftwareContext();
                var validatableSboms = context.Sboms
                    .Where(x => x.IsUsing)
                    .Include(x => x.Software)
                    .Where(x => x.Software.LocalDirectory != null)
                    .Include(x => x.Files)
                    .ThenInclude(x => x.Checksums);
                foreach (Sbom sbom in validatableSboms)
                {
                    foreach (SbomFile file in sbom.Files)
                    {
                        string path = Path.Combine(sbom.Software.LocalDirectory!, file.FileName);
                        string sha1Hash = file.Checksums.First(x => x.Algorithm == ChecksumAlgorithm.SHA1).Value;
                        string localFileHash = await ComputeSHA1Async(path, stoppingToken);
                        bool isValid = !sha1Hash.Equals(localFileHash, StringComparison.OrdinalIgnoreCase);

                        context.HashValidationResults.Add(new HashValidationResult(DateTime.Now, file, isValid));
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // TODO 実行間隔の調整
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
                builder.AppendFormat("{0:x2}", data);
            }

            return builder.ToString();
        }
    }
}
