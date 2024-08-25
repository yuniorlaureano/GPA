using GPA.Common.Entities.Invoice;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.Inventory.Configurations
{
    public class InvoiceAttachmentConfiguration : IEntityTypeConfiguration<InvoiceAttachment>
    {
        public void Configure(EntityTypeBuilder<InvoiceAttachment> builder)
        {
            builder.ToTable("InvoiceAttachments", GPASchema.INVOICE);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            builder.Property(x => x.File).IsRequired();

            builder.HasOne(x => x.Invoice)
                .WithMany(x => x.InvoiceAttachments)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
