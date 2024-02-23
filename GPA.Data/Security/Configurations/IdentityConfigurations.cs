using GPA.Data.Schemas;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GPA.Data.Security.Configurations
{
    public static class IdentityConfiguration
    {
        public static void ConfigureIdentityTables(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityUser>(b =>
            {
                b.ToTable("GPAUsers", GPASchema.SECURITY);
            });

            modelBuilder.Entity<IdentityUserClaim<string>>(b =>
            {
                b.ToTable("GPAUserClaims", GPASchema.SECURITY);
            });

            modelBuilder.Entity<IdentityUserLogin<string>>(b =>
            {
                b.ToTable("GPAUserLogin", GPASchema.SECURITY);
            });

            modelBuilder.Entity<IdentityUserToken<string>>(b =>
            {
                b.ToTable("GPAUserToken", GPASchema.SECURITY);
            });

            modelBuilder.Entity<IdentityRole>(b =>
            {
                b.ToTable("GPARoles", GPASchema.SECURITY);
            });

            modelBuilder.Entity<IdentityRoleClaim<string>>(b =>
            {
                b.ToTable("GPARoleClaims", GPASchema.SECURITY);
            });

            modelBuilder.Entity<IdentityUserRole<string>>(b =>
            {
                b.ToTable("GPAUserRoles", GPASchema.SECURITY);
            });
        }
    }
}
