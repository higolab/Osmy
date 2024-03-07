using MailKit.Net.Smtp;
using MimeKit;
using Osmy.Server.Data;
using Osmy.Server.Data.Settings;
using System.Text;

namespace Osmy.Server.Services
{
    /// <summary>
    /// アプリ通知サービス
    /// </summary>
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
            var names = dbContext.Sboms.Where(x => x.IsVulnerable)
                                       .Select(x => x.Name)
                                       .ToArray();
            if (names.Length == 0) { return; }

            if (!Settings.Notification.Email.IsEnabled) { return; };
            var contentBuilder = new StringBuilder();
            contentBuilder.AppendFormat("Vulnerabilities detected in {0} piece{1} of software.", names.Length,
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
            var names = dbContext.Sboms
                .Where(x => x.HasFileError)
                .Select(x => x.Name)
                .ToArray();
            if (names.Length == 0) { return; }

            if (!Settings.Notification.Email.IsEnabled) { return; };
            var contentBuilder = new StringBuilder();
            contentBuilder.AppendFormat("File Integrity Errors detected in {0} piece{1} of software.", names.Length,
                                        names.Length > 1 ? "s" : string.Empty);
            contentBuilder.AppendLine();
            contentBuilder.AppendLine("=====");
            foreach (var name in names)
            {
                contentBuilder.AppendLine(name);
            }
            contentBuilder.AppendLine("=====");

            SendMail("File Integrity Errors Detected", contentBuilder.ToString());
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
