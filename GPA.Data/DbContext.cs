using GPA.Common.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace GPA.Data
{
    public class DbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
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
