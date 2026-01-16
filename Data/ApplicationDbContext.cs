using AccountManager.Models;
using AccountManager.Models.Views;
using Microsoft.EntityFrameworkCore;

namespace AccountManager.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Account> Accounts => Set<Account>();

        public DbSet<VwAdminAccount> VwAdminAccounts => Set<VwAdminAccount>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // ===== Roles =====
            b.Entity<Role>(entity =>
            {
                entity.ToTable("roles");

                entity.HasKey(r => r.Id);

                entity.Property(r => r.Name)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.HasIndex(r => r.Name)
                      .IsUnique();

                // Seed: 2 role-a (stabilni ID-evi)
                entity.HasData(
                    new Role { Id = 1, Name = "User" },
                    new Role { Id = 2, Name = "Admin" }
                );
            });

            // ===== Users =====
            b.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasKey(u => u.Id);

                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.HasIndex(u => u.Email)
                      .IsUnique();

                entity.Property(u => u.Password)
                      .IsRequired();

                entity.Property(u => u.CreatedAt)
                      .IsRequired();

                // Role (1:N) - Restrict da se role ne može obrisati dok postoje useri
                entity.HasOne(u => u.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(u => u.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== Accounts =====
            b.Entity<Account>(entity =>
            {
                entity.ToTable("accounts");

                entity.HasKey(a => a.Id);

                entity.Property(a => a.FirstName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(a => a.LastName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(a => a.DateOfBirth)
                      .IsRequired();

                entity.Property(a => a.Address)
                      .IsRequired()
                      .HasMaxLength(300);

                // 1:1 User <-> Account
                entity.HasOne(a => a.User)
                      .WithOne(u => u.Account)
                      .HasForeignKey<Account>(a => a.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(a => a.UserId)
                      .IsUnique();

                
            });

            b.Entity<Account>()
                        .Property(a => a.DateOfBirth)
                        .HasColumnType("date");

            b.Entity<VwAdminAccount>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_admin_accounts");
            });
        }


    }
}


