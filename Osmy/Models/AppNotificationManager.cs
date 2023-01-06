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
    }
}
