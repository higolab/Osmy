using Microsoft.EntityFrameworkCore;
using Osmy.Models;
using Osmy.Models.HashValidation;
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
    internal class HashValidationService : QueueingBackgroundService<Sbom, HashValidationResultCollection>
    {
        /// <summary>
        /// 自動診断が必要なソフトウェアが存在するかをチェックする間隔
        /// </summary>
        public TimeSpan AutoScanCheckInterval { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// 自動診断を行う間隔
        /// </summary>
        public TimeSpan AutoScanInterval { get; set; } = TimeSpan.FromDays(1);

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var autoValidationTask = Task.Run(() => StartAutoValidationRequest(stoppingToken), stoppingToken);
            return Task.WhenAll(base.ExecuteAsync(stoppingToken), autoValidationTask);
        }

        protected override Task<HashValidationResultCollection> ProcessAsync(Sbom sbom, CancellationToken cancellationToken)
        {
            return ValidateSbomAsync(sbom, cancellationToken);
        }

        public Task<HashValidationResultCollection> Validate(Sbom sbom)
        {
            return EnqueueManual(sbom);
        }

        private async Task StartAutoValidationRequest(CancellationToken stoppingToken)
        {
            while (true)
            {
                using var context = new ManagedSoftwareContext();
                var before = DateTime.Now.Subtract(AutoScanInterval);

                // 前回スキャンから一定期間経過しているソフトウェアのIDリストを作成
                var sbomIdsNotScannedRecently = context.Sboms
                    .Where(x => x.LocalDirectory != null)
                    .Join(context.HashValidationResults,
                        sbom => sbom.Id,
                        validationResult => validationResult.SbomId,
                        (sbom, validationResult) => new { Sbom = sbom, ValidationResult = validationResult })
                    .GroupBy(x => x.Sbom)
                    .Select(x => new { SbomId = x.Key.Id, Executed = x.Select(item => item.ValidationResult).Max(item => item.Executed) })
                    .Where(x => x.Executed <= before)
                    .Select(x => x.SbomId)
                    .ToArray();

                // 一度もスキャンされていないソフトウェアのIDリストを作成
                var neverValidatedSoftwares = context.Sboms
                    .Where(x => x.LocalDirectory != null)
                    .Select(sbom => sbom.Id)
                    .Except(context.HashValidationResults.Select(x => x.SbomId).Distinct())
                    .ToArray();

                // スキャンが必要なソフトウェアのIDリストを作成
                var sbomIdsNeedScan = sbomIdsNotScannedRecently.Union(neverValidatedSoftwares).ToArray();

                foreach (var sbomId in sbomIdsNeedScan)
                {
                    var sbom = context.Sboms
                        .Include(x => x.Files)
                        .ThenInclude(x => x.Checksums)
                        .First(x => x.Id == sbomId);
                    var result = await EnqueueAuto(sbom, stoppingToken).ConfigureAwait(false);
                    context.HashValidationResults.Add(result);
                    await context.SaveChangesAsync(stoppingToken).ConfigureAwait(false);
                }

                await Task.Delay(AutoScanCheckInterval, stoppingToken).ConfigureAwait(false);
            }
        }

        private async Task<HashValidationResultCollection> ValidateSbomAsync(Sbom sbom, CancellationToken cancellationToken)
        {
            if (sbom.LocalDirectory is null) { throw new ArgumentException($"{nameof(sbom.LocalDirectory)} cannot be null"); }
            var executed = DateTime.Now;

            var results = sbom.Files.Select(async file =>
            {
                string path = Path.Combine(sbom.LocalDirectory, file.FileName);
                HashValidity result = HashValidity.FileNotFound;
                if (File.Exists(path))
                {
                    string sha1Hash = file.Checksums.First(x => x.Algorithm == ChecksumAlgorithm.SHA1).Value;
                    string localFileHash = await ComputeSHA1Async(path, cancellationToken).ConfigureAwait(false);
                    bool isValid = !sha1Hash.Equals(localFileHash, StringComparison.OrdinalIgnoreCase);
                    result = isValid ? HashValidity.Valid : HashValidity.Invalid;
                }

                return new HashValidationResult(file, result);
            });

            return new HashValidationResultCollection(executed, sbom, await Task.WhenAll(results).ConfigureAwait(false));
        }

        private static async Task<string> ComputeSHA1Async(string filePath, CancellationToken cancellationToken)
        {
            HashAlgorithm hashAlgorithm = SHA1.Create();

            using var stream = File.OpenRead(filePath);
            var localFileHash = await hashAlgorithm.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);

            var builder = new StringBuilder();
            foreach (byte data in localFileHash)
            {
                builder.AppendFormat("{0:x2}", data);
            }

            return builder.ToString();
        }
    }
}
