using Microsoft.EntityFrameworkCore;

namespace Osmy.Server.Data
{
    public static class Settings
    {
        public static TimeSpan VulnerabilityScanInterval { get; set; }

        public static TimeSpan ChecksumVerificationInterval { get; set; }

        static Settings()
        {
            using var context = new SettingDbContext();
            var setting = context.GetSetting();

            VulnerabilityScanInterval = setting.VulnerabilityScanInterval;
            ChecksumVerificationInterval = setting.ChecksumVerificationInterval;
        }

        public static void Save()
        {
            using var context = new SettingDbContext();
            var setting = context.GetSetting();

            setting.VulnerabilityScanInterval = VulnerabilityScanInterval;
            setting.ChecksumVerificationInterval = ChecksumVerificationInterval;

            context.SaveChanges();
        }
    }

    public class SettingDbContext : DbContext
    {
        public string DbPath { get; }

        public DbSet<SettingItem> Settings { get; set; }

        public SettingDbContext()
        {
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            var directory = Path.Join(appDataDir, "Osmy");
            Directory.CreateDirectory(directory);

            DbPath = Path.Join(directory, "setting.db");
            Database.EnsureCreated();   // TODO for debug InvalidOperationExceptionが発生する
        }

        public SettingItem GetSetting()
        {
            var item = Settings.FirstOrDefault();

            if (item is null)
            {
                item = new SettingItem();
                Settings.Add(item);
                SaveChanges();
            }

            return item;
        }

        public async Task<SettingItem> GetSetting(CancellationToken cancellationToken = default)
        {
            var item = await Settings.FirstOrDefaultAsync(cancellationToken);

            if (item is null)
            {
                item = new SettingItem();
                await Settings.AddAsync(item, cancellationToken);
                await SaveChangesAsync(cancellationToken);
            }

            return item;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={DbPath}");
        }
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
