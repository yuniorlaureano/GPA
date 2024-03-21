using GPA.Common.Entities.Invoice;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace GPA.Data.Invoice.Configurations
{
    public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
    {
        public void Configure(EntityTypeBuilder<Purchase> builder)
        {
            builder.HasQueryFilter(x => !x.Deleted);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasValueGenerator<SequentialGuidValueGenerator>()
                .IsRequired();

            builder.ToTable("Purchases", GPASchema.INVOICE);

            builder.HasMany(x => x.PurchaseDetailses)
                .WithOne(x => x.Purchase)
                .HasForeignKey(x => x.PurchaseId);
        }
    }
}
