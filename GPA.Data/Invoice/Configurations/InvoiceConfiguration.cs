using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.Invoice.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<GPA.Common.Entities.Invoice.Invoice>
    {
        public void Configure(EntityTypeBuilder<GPA.Common.Entities.Invoice.Invoice> builder)
        {
            builder.HasQueryFilter(x => !x.Deleted);

            builder.ToTable("Invoices", GPASchema.INVOICE);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            builder.HasMany(x => x.ClientPaymentsDetails)
                .WithOne(x => x.Invoice)
                .HasForeignKey(x => x.InvoiceId);

            builder.HasOne(x => x.Client)
                .WithMany(x => x.Invoices)
                .HasForeignKey(x => x.ClientId);

            builder.HasMany(x => x.InvoiceDetails)
                .WithOne(x => x.Invoice)
                .HasForeignKey(x => x.InvoiceId)
                .IsRequired();

        }
    }
}

