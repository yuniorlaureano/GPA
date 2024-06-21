using GPA.Common.Entities.Comon;
using GPA.Common.Entities.Inventory;
using GPA.Common.Entities.Invoice;
using GPA.Common.Entities.Security;
using GPA.Data.Security.Configurations;
using GPA.Entities.Common;
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

        //INVENTORY
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductLocation> ProductLocations { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<Reason> Reasons { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<StockDetails> StockDetails { get; set; }
        public DbSet<Store> Store { get; set; }
        public DbSet<StockCycle> StockCycle { get; set; }
        public DbSet<StockCycleDetail> StockCycleDetails { get; set; }
        public DbSet<Addon> Addons { get; set; }
        public DbSet<ProductAddon> ProductAddons { get; set; }

        //INVOICE
        public DbSet<Client> Client { get; set; }
        public DbSet<ClientPaymentsDetails> ClientPaymentsDetails { get; set; }
        public DbSet<GPA.Common.Entities.Invoice.Invoice> Invoices { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseDetails> PurchaseDetails { get; set; }
        public DbSet<InvoiceDetails> InvoiceDetails { get; set; }
        public DbSet<StorePaymentsDetails> StorePaymentsDetails { get; set; }
        public DbSet<ClientCredit> ClientCredits { get; set; }

        //SECURITY ADDED DINAMICALLY BY EF-CORE

        //COMMON
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<Unit> Units { get; set; }
    }
}
