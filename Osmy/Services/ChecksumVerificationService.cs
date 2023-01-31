using Microsoft.EntityFrameworkCore;
using Osmy.Models;
using Osmy.Models.ChecksumVerification;
using Osmy.Models.Sbom;
using Osmy.Properties;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Osmy.Services
{
    internal class ChecksumVerificationService : QueueingBackgroundService<Sbom, ChecksumVerificationResultCollection>
    {
        private readonly IAppNotificationService _appNotificationService;

        /// <summary>
        /// 自動診断が必要なソフトウェアが存在するかをチェックする間隔
        /// </summary>
        public TimeSpan AutoScanCheckInterval { get; set; } = TimeSpan.FromMinutes(5);

        public ChecksumVerificationService(IAppNotificationService appNotificationService)
        {
            _appNotificationService = appNotificationService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var autoValidationTask = Task.Run(() => StartAutoValidationRequest(stoppingToken), stoppingToken);
            return Task.WhenAll(base.ExecuteAsync(stoppingToken), autoValidationTask);
        }

        protected override Task<ChecksumVerificationResultCollection> ProcessAsync(Sbom sbom, CancellationToken cancellationToken)
        {
            return VerifyChecksumAsync(sbom, cancellationToken);
        }

        public Task<ChecksumVerificationResultCollection> Verify(Sbom sbom)
        {
            return EnqueueManual(sbom);
        }

        private async Task StartAutoValidationRequest(CancellationToken stoppingToken)
        {
            while (true)
            {
                using var context = new ManagedSoftwareContext();
                var before = DateTime.Now.Subtract(Settings.Default.ChecksumVerificationInterval);

                // 前回検証から一定期間経過しているソフトウェアのIDリストを作成
                var sbomIdsNotVerifiedRecently = context.Sboms
                    .Where(x => x.LocalDirectory != null)
                    .Join(context.ChecksumVerificationResults,
                        sbom => sbom.Id,
                        validationResult => validationResult.SbomId,
                        (sbom, verificationResult) => new { Sbom = sbom, VerificationResult = verificationResult })
                    .GroupBy(x => x.Sbom)
                    .Select(x => new { SbomId = x.Key.Id, Executed = x.Select(item => item.VerificationResult).Max(item => item.Executed) })
                    .Where(x => x.Executed <= before)
                    .Select(x => x.SbomId)
                    .ToArray();

                // 一度もスキャンされていないソフトウェアのIDリストを作成
                var neverVerifiedSoftwares = context.Sboms
                    .Where(x => x.LocalDirectory != null)
                    .Select(sbom => sbom.Id)
                    .Except(context.ChecksumVerificationResults.Select(x => x.SbomId).Distinct())
                    .ToArray();

                // スキャンが必要なソフトウェアのIDリストを作成
                var sbomIdsNeedScan = sbomIdsNotVerifiedRecently.Union(neverVerifiedSoftwares).ToArray();

                foreach (var sbomId in sbomIdsNeedScan)
                {
                    var sbom = context.Sboms
                        .Include(x => x.Files)
                        .ThenInclude(x => x.Checksums)
                        .First(x => x.Id == sbomId);
                    var result = await EnqueueAuto(sbom, stoppingToken).ConfigureAwait(false);
                    context.ChecksumVerificationResults.Add(result);
                }

                await context.SaveChangesAsync(stoppingToken).ConfigureAwait(false);
                _appNotificationService.NotifyChecksumMismatch();

                await Task.Delay(AutoScanCheckInterval, stoppingToken).ConfigureAwait(false);
            }
        }

        private async Task<ChecksumVerificationResultCollection> VerifyChecksumAsync(Sbom sbom, CancellationToken cancellationToken)
        {
            if (sbom.LocalDirectory is null) { throw new ArgumentException($"{nameof(sbom.LocalDirectory)} cannot be null"); }
            var executed = DateTime.Now;

            var results = sbom.Files.Select(async file =>
            {
                string path = Path.Combine(sbom.LocalDirectory, file.FileName);
                ChecksumCorrectness result = ChecksumCorrectness.FileNotFound;
                if (File.Exists(path))
                {
                    string sha1Hash = file.Checksums.First(x => x.Algorithm == ChecksumAlgorithm.SHA1).Value;
                    string localFileHash = await ComputeSHA1Async(path, cancellationToken).ConfigureAwait(false);
                    bool isValid = sha1Hash.Equals(localFileHash, StringComparison.OrdinalIgnoreCase);
                    result = isValid ? ChecksumCorrectness.Correct : ChecksumCorrectness.Incorrect;
                }

                return new ChecksumVerificationResult(file, result);
            });

            return new ChecksumVerificationResultCollection(executed, sbom, await Task.WhenAll(results).ConfigureAwait(false));
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
