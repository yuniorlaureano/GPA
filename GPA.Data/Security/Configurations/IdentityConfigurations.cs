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
                b.ToTable("GPAUsers", GPASchema.SECURITY);
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
                b.HasMany(p => p.UserRoles)
                    .WithOne(p => p.User)
                    .HasForeignKey(p => p.UserId);

                b.HasMany(p => p.Profiles)
                    .WithOne(p => p.User)
                    .HasForeignKey(p => p.UserId);
            });

            modelBuilder.Entity<GPAUserProfile>(b =>
            {
                b.ToTable("GPAUserProfiles", GPASchema.SECURITY);
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
                b.HasOne(p => p.User)
                    .WithMany(p => p.Profiles)
                    .HasForeignKey(p => p.UserId);
            });

            modelBuilder.Entity<GPAUserClaim>(b =>
            {
                b.ToTable("GPAUserClaims", GPASchema.SECURITY);
                b.HasKey(x => x.Id);
            });

            modelBuilder.Entity<GPAUserLogin>(b =>
            {
                b.ToTable("GPAUserLogin", GPASchema.SECURITY);
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
                b.HasOne(p => p.User)
                    .WithMany(p => p.UserLogins)
                    .HasForeignKey(p => p.UserId);
            });

            modelBuilder.Entity<GPAUserToken>(b =>
            {
                b.ToTable("GPAUserToken", GPASchema.SECURITY);
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
                b.HasOne(p => p.User)
                    .WithMany(p => p.UserTokens)
                    .HasForeignKey(p => p.UserId);
            });

            modelBuilder.Entity<GPARole>(b =>
            {
                b.ToTable("GPARoles", GPASchema.SECURITY);
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
                b.ToTable("GPARoleClaims", GPASchema.SECURITY);
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).UseIdentityColumn();

                b.HasOne(p => p.Role)
                    .WithMany(p => p.RoleClaims)
                    .HasForeignKey(p => p.RoleId);

            });

            modelBuilder.Entity<GPAUserRole>(b =>
            {
                b.ToTable("GPAUserRoles", GPASchema.SECURITY);
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
