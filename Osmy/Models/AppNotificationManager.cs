using Microsoft.EntityFrameworkCore;
using Microsoft.Toolkit.Uwp.Notifications;
using Osmy.Services;
using System.Linq;

namespace Osmy.Models
{
    /// <summary>
    /// アプリ通知マネージャー
    /// </summary>
    /// <see cref="https://learn.microsoft.com/ja-jp/windows/apps/design/shell/tiles-and-notifications/send-local-toast?tabs=desktop"/>
    internal static class AppNotificationManager
    {
        /// <summary>
        /// 脆弱性通知タグ
        /// </summary>
        const string VulnsNotifyTag = nameof(VulnerabilityScanService);

        /// <summary>
        /// チェックサム不一致通知タグ
        /// </summary>
        const string ChecksumMismatchNotifyTag = nameof(HashValidationService);

        /// <summary>
        /// 検出された脆弱性を通知します．
        /// </summary>
        public static void NotifyVulnerability()
        {
            using var dbContext = new ManagedSoftwareContext();
            var names = dbContext.ScanResults
                .Include(x => x.Sbom)
                .GroupBy(x => x.SbomId)
                .AsEnumerable()
                .Select(g => g.OrderByDescending(x => x.Executed).First())
                .Where(x => x.IsVulnerable)
                .Select(x => x.Sbom.Name)
                .ToArray();
            if (names.Length == 0) { return; }

            var toastBuilder = new ToastContentBuilder();
            toastBuilder.AddText($"Vulnerabilities detected in {names.Length} software{(names.Length > 1 ? "s" : null)}");
            toastBuilder.Show(toast => { toast.Tag = VulnsNotifyTag; });
        }

        /// <summary>
        /// チェックサムの不一致を通知します．
        /// </summary>
        public static void NotifyChecksumMismatch()
        {
            using var dbContext = new ManagedSoftwareContext();
            var names = dbContext.HashValidationResults
                .Include(x => x.Sbom)
                .GroupBy(x => x.SbomId)
                .AsEnumerable()
                .Select(g => g.OrderByDescending(x => x.Executed).First())
                .Where(x => x.HasError)
                .Select(x => x.Sbom.Name)
                .ToArray();
            if (names.Length == 0) { return; }

            var toastBuilder = new ToastContentBuilder();
            toastBuilder.AddText($"Checksum mismatch detected in {names.Length} software{(names.Length > 1 ? "s" : null)}");
            toastBuilder.Show(toast => { toast.Tag = ChecksumMismatchNotifyTag; });
        }
    }
}
