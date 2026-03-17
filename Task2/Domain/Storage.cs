using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;

namespace Domain
{
    public class Storage : DbContext
    {
        public Storage()
        {
        }

        public Storage(DbContextOptions<Storage> options) : base(options)
        {
        }

        public DbSet<Operation> Operations { get; set; }

        public DbSet<Detail> Details { get; set; }

        public DbSet<Production> Productions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
                return;

            var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (currentDirectory != null &&
                   !currentDirectory.GetFiles("*.sln").Any())
            {
                currentDirectory = currentDirectory.Parent;
            }

            var solutionPath = currentDirectory.FullName;
            var dataFolder = Path.Combine(solutionPath, "Data");

            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }

            var dbPath = Path.Combine(dataFolder, "production.db");

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Production>()
                .HasOne(p => p.Detail)
                .WithMany(d => d.Productions)
                .HasForeignKey(p => p.DetailCode)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Production>()
                .HasOne(p => p.Operation)
                .WithMany(o => o.Productions)
                .HasForeignKey(p => p.OperationCode)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}