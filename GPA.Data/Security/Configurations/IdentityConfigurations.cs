using GPA.Common.Entities.Security;
using GPA.Data.Schemas;
using GPA.Entities.Security;
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
                b.Property(b => b.Invited).IsRequired();

                b.HasIndex(b => b.Email).IsUnique();
                b.HasIndex(b => b.UserName).IsUnique();

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

            modelBuilder.Entity<InvitationToken>(b =>
            {
                b.ToTable("InvitationTokens", GPASchema.SECURITY);
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
                b.Property(x => x.UserId).IsRequired();
                b.Property(x => x.Token);
                b.Property(x => x.Expiration).IsRequired();
                b.Property(x => x.Revoked).IsRequired();
                b.Property(x => x.CreatedBy).IsRequired();
                b.Property(x => x.CreatedAt).IsRequired();

                b.HasOne(p => p.User)
                    .WithMany(p => p.InvitationTokens)
                    .HasForeignKey(p => p.UserId);
            });
        }
    }
}
