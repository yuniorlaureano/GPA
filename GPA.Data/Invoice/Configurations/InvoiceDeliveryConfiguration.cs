using GPA.Common.Entities.Invoice;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.Invoice.Configurations
{
    public class InvoiceDeliveryConfiguration : IEntityTypeConfiguration<InvoiceDelivery>
    {
        public void Configure(EntityTypeBuilder<InvoiceDelivery> builder)
        {
            builder.ToTable("InvoiceDeliveries", GPASchema.INVOICE);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();
        }
    }
}
