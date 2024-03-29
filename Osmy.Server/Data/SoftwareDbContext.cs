﻿using Microsoft.EntityFrameworkCore;
using Osmy.Core.Configuration;
using Osmy.Server.Data.Sbom;

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

        public DbSet<SbomPackageComponent> Packages { get; set; }

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
            //options.LogTo(Console.WriteLine, LogLevel.Information);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // パッケージ情報と脆弱性データは多対多
            modelBuilder.Entity<SbomPackageComponent>()
                .HasMany(e => e.Vulnerabilities)
                .WithMany(e => e.SbomPackageComponents)
                .UsingEntity<SbomPackageVulnerabilityPair>();

            // 以下はDeleteBehaviorの設定のために記述
            // MEMO: プロパティに属性を記述する方法はエラーが出て動かなかった
            modelBuilder.Entity<Sbom.Sbom>()
                .HasOne(e => e.RawSbom)
                .WithOne()
                .HasForeignKey<RawSbom>(e => e.SbomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Sbom.Sbom>()
                .HasMany(e => e.Files)
                .WithOne(e => e.Sbom)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SbomFileComponent>()
                .HasMany(e => e.Checksums)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Sbom.Sbom>()
                .HasMany(e => e.Packages)
                .WithOne()
                .HasForeignKey(e => e.SbomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Sbom.Sbom>()
                .HasMany(e => e.ExternalReferences)
                .WithOne()
                .HasForeignKey(e => e.SbomId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
