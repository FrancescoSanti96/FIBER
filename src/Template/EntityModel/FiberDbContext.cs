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
        private record UserColtureDto(int IdUser, int IdColture);
        private record UserProvinceDto(int IdUser, int IdProvince);

        public FiberDbContext(DbContextOptions<FiberDbContext> options) : base(options)
        {
            var contextStateFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "SolutionItems", "FiberDbState");
            if (!Directory.Exists(contextStateFolderPath))
            {
                throw new DirectoryNotFoundException();
            }

            LoadFromFile(contextStateFolderPath);

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
                    j => j.HasOne<Province>().WithMany().HasForeignKey("ProvinceId").OnDelete(DeleteBehavior.ClientNoAction),
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

        private void LoadFromFile(string contextStateFolderPath)
        {
            #region Seed entità

            SeedEntityFromJson<User>(contextStateFolderPath);
            SeedEntityFromJson<Role>(contextStateFolderPath);
            SeedEntityFromJson<Colture>(contextStateFolderPath);
            SeedEntityFromJson<Province>(contextStateFolderPath);

            #endregion

            #region Seed navigation properties

            var users = Users
                .Include(u => u.Coltures)
                .Include(u => u.Provinces)
                .ToList();

            var coltureMap = Coltures.ToDictionary(c => c.Id);
            var provinceMap = Provinces.ToDictionary(c => c.Id);

            // Pulizia relazioni esistenti
            foreach (var user in users)
            {
                user.Coltures.Clear();
                user.Provinces.Clear();
            }

            if (File.Exists(Path.Combine(contextStateFolderPath, "UserColture.json")))
            {
                var userColturesJson = File.ReadAllText(Path.Combine(contextStateFolderPath, "UserColture.json"));
                var userColtureOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var userColtures = JsonSerializer.Deserialize<List<UserColtureDto>>(userColturesJson, userColtureOptions);

                foreach (var uc in userColtures!)
                {
                    var user = users.FirstOrDefault(u => u.Id == uc.IdUser);
                    if (user != null && coltureMap.TryGetValue(uc.IdColture, out var colture))
                    {
                        user.Coltures.Add(colture);
                    }
                }
            }

            if (File.Exists(Path.Combine(contextStateFolderPath, "UserProvince.json")))
            {
                var userProvincesJson = File.ReadAllText(Path.Combine(contextStateFolderPath, "UserProvince.json"));
                var userProvinceOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var userProvinces = JsonSerializer.Deserialize<List<UserProvinceDto>>(userProvincesJson, userProvinceOptions);

                foreach (var up in userProvinces!)
                {
                    var user = users.FirstOrDefault(u => u.Id == up.IdUser);
                    if (user != null && provinceMap.TryGetValue(up.IdProvince, out var province))
                    {
                        user.Provinces.Add(province);
                    }
                }
            }

            #endregion

            SaveChanges();
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
                
                // Configura opzioni di serializzazione case-insensitive
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var data = JsonSerializer.Deserialize<List<T>>(json, jsonOptions);

                if (data != null)
                {
                    dbSet.AddRange(data);
                    this.SaveChanges();
                }
            }
        }
    }
}
