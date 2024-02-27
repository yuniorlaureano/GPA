using GPA.Common.Entities.Inventory;
using GPA.Common.Entities.Security;
using GPA.Data.Security.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace GPA.Data
{
    public class GPADbContext : IdentityDbContext<GPAUser, GPARole, Guid, GPAUserClaim, GPAUserRole, GPAUserLogin, GPARoleClaim, GPAUserToken>
    {
        public GPADbContext(DbContextOptions<GPADbContext> dbContextOptions) : base(dbContextOptions)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ConfigureIdentityTables();
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());//this line register all the configurations
            modelBuilder.Seed();
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductLocation> ProductLocations { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<ProviderAddress> ProviderAddresses { get; set; }
    }
}
