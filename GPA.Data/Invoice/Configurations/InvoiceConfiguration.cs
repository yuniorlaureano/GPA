using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace GPA.Data.Invoice.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<GPA.Common.Entities.Invoice.Invoice>
    { 
        public void Configure(EntityTypeBuilder<GPA.Common.Entities.Invoice.Invoice> builder)
        {
            builder.HasQueryFilter(x => !x.Deleted);

            builder.ToTable("Invoices", GPASchema.INVOICE);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasValueGenerator<SequentialGuidValueGenerator>()
                .IsRequired();

            builder.HasMany(x => x.ClientPaymentsDetails)
                .WithOne(x => x.Invoice)
                .HasForeignKey(x => x.InvoiceId);

            builder.HasOne(x => x.Sell)
                .WithOne(x => x.Invoice)
                .HasForeignKey<GPA.Common.Entities.Invoice.Invoice>(x => x.SellId);

            builder.HasOne(x => x.Client)
                .WithMany(x => x.Invoices)
                .HasForeignKey(x => x.ClientId);

        }
    }
}

