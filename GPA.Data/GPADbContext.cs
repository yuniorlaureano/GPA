using GPA.Common.Entities.Comon;
using GPA.Common.Entities.Inventory;
using GPA.Common.Entities.Invoice;
using GPA.Common.Entities.Security;
using GPA.Data.Security.Configurations;
using GPA.Entities.General;
using GPA.Entities.Inventory;
using GPA.Entities.Report;
using GPA.Entities.Security;
using GPA.Entities.Unmapped.Audit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace GPA.Data
{
    public class GPADbContext : IdentityDbContext<GPAUser, IdentityRole<Guid>, Guid, IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
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
        public DbSet<StockAttachment> StockAttachments { get; set; }
        public DbSet<RelatedProduct> RelatedProducts { get; set; }

        //INVOICE
        public DbSet<Client> Client { get; set; }
        public DbSet<ClientPaymentsDetails> ClientPaymentsDetails { get; set; }
        public DbSet<GPA.Common.Entities.Invoice.Invoice> Invoices { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseDetails> PurchaseDetails { get; set; }
        public DbSet<InvoiceDetails> InvoiceDetails { get; set; }
        public DbSet<StorePaymentsDetails> StorePaymentsDetails { get; set; }
        public DbSet<ClientCredit> ClientCredits { get; set; }
        public DbSet<InvoiceDetailsAddon> InvoiceDetailsAddons { get; set; }
        public DbSet<InvoiceAttachment> InvoiceAttachments { get; set; }

        //SECURITY ADDED DINAMICALLY BY EF-CORE
        public DbSet<GPAProfile> Profiles { get; set; }
        public DbSet<GPAUserProfile> UserProfile { get; set; }
        public DbSet<InvitationToken> InvitationTokens { get; set; }

        //COMMON
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<EmailConfiguration> EmailConfigurations { get; set; }
        public DbSet<BlobStorageConfiguration> BlobStorageConfigurations { get; set; }
        public DbSet<PrintInformation> PrintInformation { get; set; }
        public DbSet<ReportTemplate> ReportTemplates { get; set; }

        //AUDIT
        public DbSet<AddonHistory> AddonHistory { get; set; }
        public DbSet<ClientHistory> ClientHistory { get; set; }
        public DbSet<InvoiceHistory> InvoiceHistory { get; set; }
        public DbSet<ProductAddonHistory> ProductAddonHistory { get; set; }
        public DbSet<ProductHistory> ProductHistory { get; set; }
        public DbSet<ProfileHistory> ProfileHistory { get; set; }
        public DbSet<StockHistory> StockHistory { get; set; }
        public DbSet<UserHistory> UserHistory { get; set; }
        public DbSet<UserProfileHistory> UserProfileHistory { get; set; }
    }
}
