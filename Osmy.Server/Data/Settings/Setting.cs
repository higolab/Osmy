using Osmy.Core.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Osmy.Server.Data.Settings
{
    public static class Settings
    {
        private static string ConfigDirectory => DefaultServerConfig.ConfigDirectory;

        public static GeneralSetting General { get; set; }

        public static NotificationSetting Notification { get; set; }

        static Settings()
        {
            Load();
        }

        [MemberNotNull(nameof(General), nameof(Notification))]
        public static void Load()
        {
            // load setting from setting files (or load default settings if not exists)
            General = LoadSettingFile<GeneralSetting>(DefaultServerConfig.GeneralSettingFileName);
            Notification = LoadSettingFile<NotificationSetting>(DefaultServerConfig.NotificationSettingFileName);
            
            // by saving loaded setting, create default setting files if not exists
            Save();
        }

        public static void Save()
        {
            EnsureConfigDirecotryCreated();
            SaveSettingFile(General, DefaultServerConfig.GeneralSettingFileName);
            SaveSettingFile(Notification, DefaultServerConfig.NotificationSettingFileName);
        }

        private static void EnsureConfigDirecotryCreated()
        {
            Directory.CreateDirectory(ConfigDirectory);
        }

        private static T LoadSettingFile<T>(string fileName) where T : new()
        {
            var path = Path.Combine(ConfigDirectory, fileName);
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

        private static void SaveSettingFile<T>(T setting, string fileName)
        {
            var path = Path.Combine(ConfigDirectory, fileName);
            using var stream = new FileStream(path, FileMode.Create);
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
            JsonSerializer.Serialize(writer, setting);
        }
    }
}
