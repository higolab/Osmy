using Microsoft.EntityFrameworkCore;
using Osmy.Gui.Models.ChecksumVerification;
using Osmy.Gui.Models.Sbom;
using Osmy.Gui.Models.Sbom.Spdx;
using System;
using System.IO;

namespace Osmy.Gui.Models
{
    internal class ManagedSoftwareContext : DbContext
    {
        public string DbPath { get; }

        public DbSet<Sbom.Sbom> Sboms { get; set; }

        public DbSet<VulnerabilityScanResult> ScanResults { get; set; }

        public DbSet<ChecksumVerificationResultCollection> ChecksumVerificationResults { get; set; }

        public ManagedSoftwareContext()
        {
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var directory = Path.Join(appDataDir, "Osmy");
            DbPath = Path.Join(directory, "softwares.db");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            Database.EnsureCreated();   // TODO for debug InvalidOperationExceptionが発生する
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={DbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Sbom.Sbom>()
                .HasDiscriminator()
                .HasValue<Spdx>("sbom_spdx");

            modelBuilder.Entity<SbomPackage>()
                .HasDiscriminator()
                .HasValue<SpdxSoftwarePackage>("package_spdx");

            modelBuilder.Entity<SbomExternalReference>()
                .HasDiscriminator()
                .HasValue<SpdxExternalReference>("external_ref_spdx");
        }
    }
}
