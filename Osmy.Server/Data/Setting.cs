using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Osmy.Server.Data
{
    public static class Settings
    {
        private static string SettingDirectory { get; }

        public static CommonSetting Common { get; set; }

        public static NotificationSetting Notification { get; set; }

        static Settings()
        {
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            SettingDirectory = Path.Join(appDataDir, "Osmy", "Setting");

            Load();
        }

        [MemberNotNull(nameof(Common), nameof(Notification))]
        public static void Load()
        {
            Common = LoadSettingFile<CommonSetting>(nameof(Common));
            Notification = LoadSettingFile<NotificationSetting>(nameof(Notification));
            Save(); // TODO for debug
        }

        public static void Save()
        {
            EnsureSettingDirecotryCreated();
            SaveSettingFile(Common, nameof(Common));
            SaveSettingFile(Notification, nameof(Notification));
        }

        private static void EnsureSettingDirecotryCreated()
        {
            Directory.CreateDirectory(SettingDirectory);
        }

        private static T LoadSettingFile<T>(string name) where T : new()
        {
            var path = Path.Combine(SettingDirectory, $"{name}.json");
            if (File.Exists(path))
            {
                using var stream = File.OpenRead(path);
                return JsonSerializer.Deserialize<T>(stream) ?? new T();
            }
            else
            {
                return new T();
            }
        }

        private static void SaveSettingFile<T>(T setting, string name)
        {
            var path = Path.Combine(SettingDirectory, $"{name}.json");
            using var stream = File.OpenWrite(path);
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
            JsonSerializer.Serialize(writer, setting);
        }
    }

    public class CommonSetting
    {
        public TimeSpan VulnerabilityScanInterval { get; set; } = TimeSpan.FromDays(1);

        public TimeSpan ChecksumVerificationInterval { get; set; } = TimeSpan.FromDays(1);
    }

    public class NotificationSetting
    {
        public EmailNotificationSetting Email { get; set; } = new EmailNotificationSetting();
    }

    public class EmailNotificationSetting
    {
        public bool IsEnabled { get; set; }

        public string Host { get; set; } = string.Empty;

        public int Port { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string[] To { get; set; } = Array.Empty<string>();

        public string[] Cc { get; set; } = Array.Empty<string>();

        public string[] Bcc { get; set; } = Array.Empty<string>();
    }

    public class SettingItem
    {
        public int Id { get; set; }

        public TimeSpan VulnerabilityScanInterval { get; set; }

        public TimeSpan ChecksumVerificationInterval { get; set; }

        public SettingItem(TimeSpan vulnerabilityScanInterval, TimeSpan checksumVerificationInterval)
        {
            VulnerabilityScanInterval = vulnerabilityScanInterval;
            ChecksumVerificationInterval = checksumVerificationInterval;
        }

        public SettingItem() : this(TimeSpan.FromDays(1), TimeSpan.FromDays(1)) { }
    }
}
