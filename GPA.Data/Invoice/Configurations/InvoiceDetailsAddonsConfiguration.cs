using GPA.Common.Entities.Invoice;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.Invoice.Configurations
{
    public class InvoiceDetailsAddonsConfiguration : IEntityTypeConfiguration<InvoiceDetailsAddon>
    {
        public void Configure(EntityTypeBuilder<InvoiceDetailsAddon> builder)
        {
            builder.ToTable("InvoiceDetailsAddons", GPASchema.INVOICE);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            builder.Property(x => x.Value).HasColumnType("decimal(18,2)").IsRequired();

            builder.HasOne(x => x.InvoiceDetails)
                .WithMany(x => x.InvoiceDetailsAddons)
                .HasForeignKey(x => x.InvoiceDetailId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
