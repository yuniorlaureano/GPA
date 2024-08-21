using GPA.Common.Entities.Security;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data.Security.Configurations
{
    public static class IdentityConfiguration
    {
        public static void ConfigureIdentityTables(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GPAUser>(b =>
            {
                b.ToTable("Users", GPASchema.SECURITY);
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
                b.Property(b => b.UserName).HasMaxLength(100);
                b.Property(b => b.LastName).HasMaxLength(100);
                b.Property(b => b.LastTOTPCode).HasMaxLength(256).IsRequired(false);
                b.Property(b => b.TOTPAccessCodeAttempts).HasDefaultValue(0);
                b.Property(b => b.TOTPAccessCodeAttemptsDate);
                b.Property(b => b.PasswordHash).HasMaxLength(256);
                b.Property(b => b.UserName).HasMaxLength(30);
                b.Property(b => b.Email).HasMaxLength(254);

                b.HasMany(p => p.UserRoles)
                    .WithOne(p => p.User)
                    .HasForeignKey(p => p.UserId);

                b.HasMany(p => p.Profiles)
                    .WithOne(p => p.User)
                    .HasForeignKey(p => p.UserId);
            });

            modelBuilder.Entity<GPAUserProfile>(b =>
            {
                b.ToTable("UserProfiles", GPASchema.SECURITY);
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
                b.HasOne(p => p.User)
                    .WithMany(p => p.Profiles)
                    .HasForeignKey(p => p.UserId);
            });

            modelBuilder.Entity<GPAUserClaim>(b =>
            {
                b.ToTable("UserClaims", GPASchema.SECURITY);
                b.HasKey(x => x.Id);
            });

            modelBuilder.Entity<GPAUserLogin>(b =>
            {
                b.ToTable("UserLogin", GPASchema.SECURITY);
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
                b.HasOne(p => p.User)
                    .WithMany(p => p.UserLogins)
                    .HasForeignKey(p => p.UserId);
            });

            modelBuilder.Entity<GPAUserToken>(b =>
            {
                b.ToTable("UserToken", GPASchema.SECURITY);
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
                b.HasOne(p => p.User)
                    .WithMany(p => p.UserTokens)
                    .HasForeignKey(p => p.UserId);
            });

            modelBuilder.Entity<GPARole>(b =>
            {
                b.ToTable("Roles", GPASchema.SECURITY);
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
                b.HasMany(p => p.UserRoles)
                    .WithOne(p => p.Role)
                    .HasForeignKey(p => p.RoleId);

                b.HasMany(p => p.RoleClaims)
                    .WithOne(p => p.Role)
                    .HasForeignKey(p => p.RoleId);

            });

            modelBuilder.Entity<GPARoleClaim>(b =>
            {
                b.ToTable("RoleClaims", GPASchema.SECURITY);
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).UseIdentityColumn();

                b.HasOne(p => p.Role)
                    .WithMany(p => p.RoleClaims)
                    .HasForeignKey(p => p.RoleId);

            });

            modelBuilder.Entity<GPAUserRole>(b =>
            {
                b.ToTable("UserRoles", GPASchema.SECURITY);
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
                //b.HasOne(p => p.Role)
                //   .WithMany(p => p.UserRoles)
                //   .HasForeignKey(p => p.RoleId);

                //b.HasOne(p => p.User)
                //   .WithMany(p => p.UserRoles)
                //   .HasForeignKey(p => p.UserId);
            });
        }
    }
}
