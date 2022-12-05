﻿using Microsoft.EntityFrameworkCore;
using Osmy.Models.Sbom;
using Osmy.Models.Sbom.Spdx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osmy.Models
{
    internal class ManagedSoftwareContext : DbContext
    {
        public string DbPath { get; }

        public DbSet<Software> Softwares { get; set; }

        public DbSet<Sbom.Sbom> Sboms { get; set; }

        public DbSet<VulnerabilityScanResult> ScanResults { get; set; }

        public ManagedSoftwareContext()
        {
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var directory = Path.Join(appDataDir, "Osmy");
            DbPath = Path.Join(directory, "softwares.db");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            Database.EnsureCreated();   // TODO for debug
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={DbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Software>()
                .HasMany(x => x.Sboms);

            var sbomEntityTypeBuilder = modelBuilder.Entity<Sbom.Sbom>();
            sbomEntityTypeBuilder.HasDiscriminator()
                .HasValue<Spdx>("sbom_spdx");
            sbomEntityTypeBuilder.HasOne(x => x.Software);

            modelBuilder.Entity<SbomPackage>()
                .HasDiscriminator()
                .HasValue<SpdxSoftwarePackage>("package_spdx");
        }
    }
}