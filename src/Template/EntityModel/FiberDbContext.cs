using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Template.EntityModel.Models;
using Template.Infrastructure;

namespace Template.EntityModel
{
    public class FiberDbContext : DbContext
    {
        public FiberDbContext(DbContextOptions<FiberDbContext> options) : base(options)
        {
            var contextStateFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "SolutionItems", "FiberDbState");
            if (!Directory.Exists(contextStateFolderPath))
            {
                throw new DirectoryNotFoundException();
            }

            SeedEntityFromJson<User>(contextStateFolderPath);
            SeedEntityFromJson<Role>(contextStateFolderPath);
            SeedEntityFromJson<Colture>(contextStateFolderPath);
            SeedEntityFromJson<Province>(contextStateFolderPath);
        }

        public DbSet<Colture> Coltures { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Bulletin> Bulletins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Province>()
                .HasIndex(p => p.Code)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            // User-Colture relationships
            modelBuilder.Entity<User>()
                .HasMany(u => u.Coltures)
                .WithMany(c => c.SubscribedUsers)
                .UsingEntity<Dictionary<string, object>>(
                    "UserColture",
                    j => j.HasOne<Colture>().WithMany().HasForeignKey("ColtureId").OnDelete(DeleteBehavior.ClientNoAction),
                    j => j.HasOne<User>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.ClientNoAction)
                );

            // User-Province relationships
            modelBuilder.Entity<User>()
                .HasMany(u => u.Provinces)
                .WithMany(p => p.SubscribedUsers)
                .UsingEntity<Dictionary<string, object>>(
                    "UserProvince",
                    j => j.HasOne<Province>().WithMany().HasForeignKey("ColtureId").OnDelete(DeleteBehavior.ClientNoAction),
                    j => j.HasOne<User>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.ClientNoAction)
                );

            // User - Role relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.ClientNoAction);

            // Bulletin relationships
            modelBuilder.Entity<Bulletin>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bulletins)
                .HasForeignKey(b => b.IdUser)
                .OnDelete(DeleteBehavior.ClientNoAction);

            modelBuilder.Entity<Bulletin>()
                .HasOne(b => b.Province)
                .WithMany(p => p.Bulletins)
                .HasForeignKey(b => b.IdProvince)
                .OnDelete(DeleteBehavior.ClientNoAction);

            modelBuilder.Entity<Bulletin>()
                .HasOne(b => b.Colture)
                .WithMany(c => c.Bulletins)
                .HasForeignKey(b => b.IdColture)
                .OnDelete(DeleteBehavior.ClientNoAction);

            // Additional configurations
            modelBuilder.Entity<Province>()
                .Property(p => p.Code)
                .HasColumnType("varchar(10)");

            modelBuilder.Entity<Bulletin>()
                .Property(b => b.ExpireDate)
                .HasColumnType("date");

            modelBuilder.Entity<Bulletin>()
                .Property(b => b.Body)
                .HasColumnType("varchar(max)");
        }

        private void SeedEntityFromJson<T>(string contextStateFolderPath) where T : class
        {
            var dbSet = this.Set<T>();

            if (!dbSet.Any())
            {
                var fileName = $"{typeof(T).Name}.json";
                var fullPath = Path.Combine(contextStateFolderPath, fileName);

                if (!File.Exists(fullPath))
                    throw new FileNotFoundException($"File not found: {fullPath}");

                var json = File.ReadAllText(fullPath);
                var data = JsonSerializer.Deserialize<List<T>>(json);

                if (data != null)
                {
                    dbSet.AddRange(data);
                    this.SaveChanges();
                }
            }
        }
    }
}
