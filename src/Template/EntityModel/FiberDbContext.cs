using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Template.EntityModel.Models;

namespace Template.EntityModel
{
    public class FiberDbContext : DbContext
    {
        public record UserColtureDto(int IdUser, int IdColture);
        public record UserProvinceDto(int IdUser, int IdProvince);
        public record BulletinColtureDto(int IdBulletin, int IdColture);
        public record BulletinProvinceDto(int IdBulletin, int IdProvince);

        public FiberDbContext(DbContextOptions<FiberDbContext> options) : base(options) { }

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
                .HasMany(b => b.Coltures)
                .WithMany(c => c.Bulletins)
                .UsingEntity<Dictionary<string, object>>(
                    "BulletinColture",
                    j => j.HasOne<Colture>().WithMany().HasForeignKey("ColtureId").OnDelete(DeleteBehavior.ClientNoAction),
                    j => j.HasOne<Bulletin>().WithMany().HasForeignKey("BulletinId").OnDelete(DeleteBehavior.ClientNoAction)
                );

            modelBuilder.Entity<Bulletin>()
                .HasMany(b => b.Provinces)
                .WithMany(p => p.Bulletins)
                .UsingEntity<Dictionary<string, object>>(
                    "BulletinProvince",
                    j => j.HasOne<Province>().WithMany().HasForeignKey("ColtureId").OnDelete(DeleteBehavior.ClientNoAction),
                    j => j.HasOne<Bulletin>().WithMany().HasForeignKey("BulletinId").OnDelete(DeleteBehavior.ClientNoAction)
                );

            modelBuilder.Entity<Bulletin>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bulletins)
                .HasForeignKey(b => b.IdUser)
                .OnDelete(DeleteBehavior.ClientNoAction);

            // Additional configurations
            modelBuilder.Entity<Province>()
                .Property(p => p.Code)
                .HasColumnType("varchar(10)");

            modelBuilder.Entity<Bulletin>()
                .Property(b => b.Body)
                .HasColumnType("text");

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FirstName = "Giuseppe",
                    LastName = "Verdi",
                    Email = "giuseppe.verdi@agricoltura.com",
                    Password = "$2a$11$I7BWy7GEhVUMdYzQNfxBZOGUk1yvvpquxUx3Zzx76Af5Wrlay1dhK",
                    RoleId = 2,
                    LastLoggedAt = null
                },
                new User
                {
                    Id = 2,
                    FirstName = "Anna",
                    LastName = "Ferrari",
                    Email = "anna.ferrari@agricoltura.com",
                    Password = "$2a$11$gffIU.L5HqfpY2YI.tDXlutRtXpnJE6js45puOuwnYbLeE9JdQIlW",
                    RoleId = 2,
                    LastLoggedAt = null
                },
                new User
                {
                    Id = 3,
                    FirstName = "Marco",
                    LastName = "Romano",
                    Email = "marco.romano@agricoltura.com",
                    Password = "$2a$11$FjYvYHLG6Oc0k7F40D6You0bZB8wEvxJIi6VoJ/XyVbsJ1e/n27m2",
                    RoleId = 3,
                    LastLoggedAt = null
                },
                new User
                {
                    Id = 4,
                    FirstName = "Sara",
                    LastName = "Conti",
                    Email = "sara.conti@agricoltura.com",
                    Password = "$2a$11$drrlbSq0lU/rSj1y.XAaCOzV/70M.ScJhYXmNE9a4i0sal6Rzifw6",
                    RoleId = 3,
                    LastLoggedAt = null
                }
            );

            modelBuilder.Entity<Colture>().HasData(
                new Colture { Id = 1, Name = "Actinidia" },
                new Colture { Id = 2, Name = "Aglio" },
                new Colture { Id = 3, Name = "Albicocco" },
                new Colture { Id = 4, Name = "Barbabietola da zucchero" },
                new Colture { Id = 5, Name = "Carciofo" },
                new Colture { Id = 6, Name = "Carota da seme" },
                new Colture { Id = 7, Name = "Ciliegio" },
                new Colture { Id = 8, Name = "Cipolla" },
                new Colture { Id = 9, Name = "Cocomero" },
                new Colture { Id = 10, Name = "Coriandolo" },
                new Colture { Id = 11, Name = "Fagiolino" },
                new Colture { Id = 12, Name = "Girasole" },
                new Colture { Id = 13, Name = "Grano" },
                new Colture { Id = 14, Name = "Kako" },
                new Colture { Id = 15, Name = "Mais" },
                new Colture { Id = 16, Name = "Melone" },
                new Colture { Id = 17, Name = "Melo" },
                new Colture { Id = 18, Name = "Orzo" },
                new Colture { Id = 19, Name = "Patata" },
                new Colture { Id = 20, Name = "Pero" },
                new Colture { Id = 21, Name = "Pesco" },
                new Colture { Id = 22, Name = "Pisello" },
                new Colture { Id = 23, Name = "Ravanello" },
                new Colture { Id = 24, Name = "Senape" },
                new Colture { Id = 25, Name = "Soia" },
                new Colture { Id = 26, Name = "Sorgo" },
                new Colture { Id = 27, Name = "Susino" },
                new Colture { Id = 28, Name = "Vite" }
            );


            modelBuilder.Entity<Province>().HasData(
                new Province { Id = 1, Name = "Bologna", Code = "BO" },
                new Province { Id = 2, Name = "Ferrara", Code = "FE" },
                new Province { Id = 3, Name = "Forlì-Cesena", Code = "FC" },
                new Province { Id = 4, Name = "Modena", Code = "MO" },
                new Province { Id = 5, Name = "Parma", Code = "PR" },
                new Province { Id = 6, Name = "Piacenza", Code = "PC" },
                new Province { Id = 7, Name = "Ravenna", Code = "RA" },
                new Province { Id = 8, Name = "Reggio Emilia", Code = "RE" },
                new Province { Id = 9, Name = "Rimini", Code = "RN" }
            );

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Agricoltore" },
                new Role { Id = 3, Name = "Tecnico" }
            );

        }
    }
}
