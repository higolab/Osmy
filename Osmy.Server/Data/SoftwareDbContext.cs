using Microsoft.EntityFrameworkCore;
using Osmy.Core.Configuration;
using Osmy.Server.Data.Sbom;
using Osmy.Server.Services;

namespace Osmy.Server.Data
{
    internal class SoftwareDbContext : DbContext
    {
        /// <summary>
        /// DB作成済みか
        /// </summary>
        private static bool _isCreated = false;

        /// <summary>
        /// DB作成処理のロック
        /// </summary>
        private static readonly object CreationLockObj = new();

        /// <summary>
        /// DBのファイルパス
        /// </summary>
        public static string DbPath { get; }

        public DbSet<Sbom.Sbom> Sboms { get; set; }

        public DbSet<SbomFileComponent> Files { get; set; }

        public DbSet<VulnerabilityData> Vulnerabilities { get; set; }

        public DbSet<SbomExternalReferenceComponent> ExternalReferences { get; set; }

        public DbSet<SbomPackageVulnerabilityPair> PackageVulnerabilityPairs { get; set; }

        public SoftwareDbContext()
        {
            // 不必要な場合はロックを発生させない
            if (!_isCreated)
            {
                // 作成処理が複数同時実行されるのを防ぐためにロック
                lock (CreationLockObj)
                {
                    Database.EnsureCreated();
                    _isCreated = true;
                }
            }
        }

        static SoftwareDbContext()
        {
            var directory = DefaultServerConfig.DataDirectory;
            DbPath = Path.Join(directory, "softwares.db");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={DbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // SBOMと脆弱性データは多対多
            modelBuilder.Entity<SbomPackageComponent>()
                .HasMany(e => e.Vulnerabilities)
                .WithMany(e => e.SbomPackageComponents)
                .UsingEntity<SbomPackageVulnerabilityPair>();
        }
    }
}
