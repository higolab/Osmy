//using Microsoft.Toolkit.Uwp.Notifications;

using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using Osmy.Server.Data;
using System.Text;

namespace Osmy.Server.Services
{
    /// <summary>
    /// アプリ通知サービス
    /// </summary>
    /// <see cref="https://learn.microsoft.com/ja-jp/windows/apps/design/shell/tiles-and-notifications/send-local-toast?tabs=desktop"/>
    // TODO マルチプラットフォーム対応の通知方法を検討
    internal class AppNotificationService : IAppNotificationService
    {
        /// <summary>
        /// 脆弱性通知タグ
        /// </summary>
        const string VulnsNotifyTag = nameof(VulnerabilityScanService);

        /// <summary>
        /// チェックサム不一致通知タグ
        /// </summary>
        const string ChecksumMismatchNotifyTag = nameof(ChecksumVerificationService);

        public void NotifyVulnerability()
        {
            using var dbContext = new SoftwareDbContext();
            var names = dbContext.ScanResults
                .Include(x => x.Sbom)
                .GroupBy(x => x.SbomId)
                .AsEnumerable()
                .Select(g => g.OrderByDescending(x => x.Executed).First())
                .Where(x => x.IsVulnerable)
                .Select(x => x.Sbom.Name)
                .ToArray();
            if (names.Length == 0) { return; }

            if (!Settings.Notification.Email.IsEnabled) { return; };
            var contentBuilder = new StringBuilder();
            contentBuilder.AppendFormat("Vulnerabilities detected in {0} software{1}.", names.Length,
                                        names.Length > 1 ? "s" : string.Empty);
            contentBuilder.AppendLine();
            contentBuilder.AppendLine("=====");
            foreach (var name in names)
            {
                contentBuilder.AppendLine(name);
            }
            contentBuilder.AppendLine("=====");

            SendMail("Vulnerabilities Detected", contentBuilder.ToString());
        }

        public void NotifyChecksumMismatch()
        {
            using var dbContext = new SoftwareDbContext();
            var names = dbContext.ChecksumVerificationResults
                .Include(x => x.Sbom)
                .GroupBy(x => x.SbomId)
                .AsEnumerable()
                .Select(g => g.OrderByDescending(x => x.Executed).First())
                .Where(x => x.HasError)
                .Select(x => x.Sbom.Name)
                .ToArray();
            if (names.Length == 0) { return; }

            if (!Settings.Notification.Email.IsEnabled) { return; };
            var contentBuilder = new StringBuilder();
            contentBuilder.AppendFormat("Checksum mismatch detected in {0} software{1}.", names.Length,
                                        names.Length > 1 ? "s" : string.Empty);
            contentBuilder.AppendLine();
            contentBuilder.AppendLine("=====");
            foreach (var name in names)
            {
                contentBuilder.AppendLine(name);
            }
            contentBuilder.AppendLine("=====");

            SendMail("Checksum Mismatch Detected", contentBuilder.ToString());
        }

        private static void SendMail(string title, string content)
        {
            var msg = new MimeMessage();
            var from = $"{Settings.Notification.Email.Username}@{Settings.Notification.Email.Host}";
            msg.From.Add(new MailboxAddress("Osmy", from));

            // to
            foreach (var to in Settings.Notification.Email.To)
            {
                msg.To.Add(new MailboxAddress(to, to));
            }

            // cc
            foreach (var cc in Settings.Notification.Email.Cc)
            {
                msg.Cc.Add(new MailboxAddress(cc, cc));
            }

            // bcc
            foreach (var bcc in Settings.Notification.Email.Bcc)
            {
                msg.Bcc.Add(new MailboxAddress(bcc, bcc));
            }

            msg.Subject = $"Osmy Notification - {title}";
            var text = new TextPart("Plain")
            {
                Text = content
            };
            msg.Body = text;

            using var client = new SmtpClient();
            client.Connect(Settings.Notification.Email.Host, Settings.Notification.Email.Port, MailKit.Security.SecureSocketOptions.Auto);
            client.Authenticate(Settings.Notification.Email.Username, Settings.Notification.Email.Password);
            client.Send(msg);
        }
    }

    /// <summary>
    /// アプリ通知サービス
    /// </summary>
    internal interface IAppNotificationService
    {
        /// <summary>
        /// 検出された脆弱性を通知します．
        /// </summary>
        void NotifyVulnerability();

        /// <summary>
        /// チェックサムの不一致を通知します．
        /// </summary>
        void NotifyChecksumMismatch();
    }
}
