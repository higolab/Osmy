using Microsoft.EntityFrameworkCore;
using Osmy.Server.Data;
using Osmy.Server.Data.ChecksumVerification;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Osmy.Server.Services
{
    internal class ChecksumVerificationService : QueueingBackgroundService<long>
    {
        //private readonly IAppNotificationService _appNotificationService;

        /// <summary>
        /// 自動診断が必要なソフトウェアが存在するかをチェックする間隔
        /// </summary>
        public TimeSpan AutoScanCheckInterval { get; set; } = TimeSpan.FromMinutes(5);

        public ChecksumVerificationService(/*IAppNotificationService appNotificationService*/)
        {
            //_appNotificationService = appNotificationService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var autoValidationTask = Task.Run(() => StartAutoValidationRequest(stoppingToken), stoppingToken);
            return Task.WhenAll(base.ExecuteAsync(stoppingToken), autoValidationTask);
        }

        protected override Task ProcessAsync(long sbomId, CancellationToken cancellationToken)
        {
            return VerifyChecksumAsync(sbomId, cancellationToken);
        }

        public Task Verify(long sbomId)
        {
            return EnqueueManual(sbomId);
        }

        private async Task StartAutoValidationRequest(CancellationToken stoppingToken)
        {
            while (true)
            {
                using (var context = new SoftwareDbContext())
                {
                    var before = DateTime.Now.Subtract(Settings.ChecksumVerificationInterval);

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
                        await EnqueueAuto(sbomId, stoppingToken);
                    }
                }

                //_appNotificationService.NotifyChecksumMismatch();

                await Task.Delay(AutoScanCheckInterval, stoppingToken);
            }
        }

        private static async Task VerifyChecksumAsync(long sbomId, CancellationToken cancellationToken)
        {
            var executed = DateTime.Now;

            using var dbContext = new SoftwareDbContext();
            var sbom = await dbContext.Sboms.Include(x => x.Files)
                .ThenInclude(x => x.Checksums)
                .FirstOrDefaultAsync(x => x.Id == sbomId, cancellationToken)
                ?? throw new InvalidOperationException($"SBOM(id={sbomId}) is not found");
            if (sbom.LocalDirectory is null)
            {
                throw new ArgumentException($"{nameof(sbom.LocalDirectory)} cannot be null");
            }

            var results = sbom.Files.Select(async file =>
            {
                string path = Path.Combine(sbom.LocalDirectory, file.FileName);
                var result = Core.Data.Sbom.ChecksumVerification.ChecksumCorrectness.FileNotFound;
                if (File.Exists(path))
                {
                    string sha1Hash = file.Checksums.First(x => x.Algorithm == Core.Data.Sbom.ChecksumAlgorithm.SHA1).Value;
                    byte[] localFileHash = await ComputeSHA1Async(path, cancellationToken);
                    bool isValid = CompareHash(sha1Hash, localFileHash);
                    result = isValid ? Core.Data.Sbom.ChecksumVerification.ChecksumCorrectness.Correct : Core.Data.Sbom.ChecksumVerification.ChecksumCorrectness.Incorrect;
                }

                return new ChecksumVerificationResult(file, result);
            });

            var resultCollection = new ChecksumVerificationResultCollection(executed, sbom, await Task.WhenAll(results));
            dbContext.ChecksumVerificationResults.Add(resultCollection);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static async Task<byte[]> ComputeSHA1Async(string filePath, CancellationToken cancellationToken)
        {
            HashAlgorithm hashAlgorithm = SHA1.Create();

            using var stream = File.OpenRead(filePath);
            var localFileHash = await hashAlgorithm.ComputeHashAsync(stream, cancellationToken);

            return localFileHash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CompareHash(string hashStr, byte[] hashBytes)
        {
            using var strEnumerator = hashStr.GetEnumerator();
            foreach (var b in hashBytes)
            {
                if (!strEnumerator.MoveNext())
                {
                    return false;
                }
                var upper = strEnumerator.Current;
                if (!CompareDigit(upper, b >> 4))
                {
                    return false;
                }

                if (!strEnumerator.MoveNext())
                {
                    return false;
                }
                var lower = strEnumerator.Current;
                if (!CompareDigit(lower, b & 0xf))
                {
                    return false;
                }
            }

            return !strEnumerator.MoveNext();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CompareDigit(char c, int b)
        {
            return c switch
            {
                '0' => b == 0,
                '1' => b == 1,
                '2' => b == 2,
                '3' => b == 3,
                '4' => b == 4,
                '5' => b == 5,
                '6' => b == 6,
                '7' => b == 7,
                '8' => b == 8,
                '9' => b == 9,
                'A' or 'a' => b == 0xa,
                'B' or 'b' => b == 0xb,
                'C' or 'c' => b == 0xc,
                'D' or 'd' => b == 0xd,
                'E' or 'e' => b == 0xe,
                'F' or 'f' => b == 0xf,
                _ => false,
            };
        }
    }
}
