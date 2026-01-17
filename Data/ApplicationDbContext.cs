using AccountManager.Models;
using AccountManager.Models.Views;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

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

        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<UserSession> UserSessions => Set<UserSession>();
        public DbSet<UserAuditLog> UserAuditLogs => Set<UserAuditLog>(); // ako ga koristiš


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

            b.Entity<Permission>(entity =>
            {
                entity.ToTable("permissions");   // ⬅ lowercase

                entity.HasKey(p => p.Id);

                entity.Property(p => p.Code)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(p => p.Name)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.HasIndex(p => p.Code)
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

            b.Entity<RolePermission>(e =>
            {
                e.ToTable("role_permissions");

                e.HasKey(x => new { x.RoleId, x.PermissionId });

                e.HasOne(x => x.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(x => x.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<Permission>().HasData(
    new Permission { Id = 1, Code = "USERS_VIEW", Name = "Pregled korisnika" },
    new Permission { Id = 2, Code = "USERS_ROLE_CHANGE", Name = "Promjena role korisnika" },
    new Permission { Id = 3, Code = "ACCOUNTS_VIEW", Name = "Pregled računa" },
    new Permission { Id = 4, Code = "AUDIT_VIEW", Name = "Pregled audita" }
);
            b.Entity<RolePermission>().HasData(
    new RolePermission { RoleId = 2, PermissionId = 1 },
    new RolePermission { RoleId = 2, PermissionId = 2 },
    new RolePermission { RoleId = 2, PermissionId = 3 },
    new RolePermission { RoleId = 2, PermissionId = 4 }
);

            b.Entity<UserSession>(e =>
            {
                e.ToTable("user_sessions");
                e.HasKey(x => x.Id);

                e.Property(x => x.Id).HasColumnType("uuid");

                e.Property(x => x.CreatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                e.Property(x => x.LastSeenAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                e.Property(x => x.RevokedAt).HasColumnType("timestamp with time zone");

                e.Property(x => x.IpAddress).HasMaxLength(50);
                e.Property(x => x.UserAgent).HasMaxLength(300);

                e.HasIndex(x => x.UserId);

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<UserAuditLog>(e =>
            {
                e.ToTable("user_audit_logs");
                e.HasKey(x => x.Id);

                e.Property(x => x.ChangedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                e.HasIndex(x => x.UserId);
            });

        }



    }
}


