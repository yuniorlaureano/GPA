using GPA.Common.Entities.Invoice;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.Invoice.Configurations
{
    public class PurchaseDetailsConfiguration : IEntityTypeConfiguration<PurchaseDetails>
    {
        public void Configure(EntityTypeBuilder<PurchaseDetails> builder)
        {
            builder.HasQueryFilter(x => !x.Deleted);

            builder.ToTable("PurchaseDetails", GPASchema.INVOICE);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            builder.HasOne(x => x.Purchase)
                .WithMany(x => x.PurchaseDetailses)
                .HasForeignKey(x => x.PurchaseId);

        }
    }
}
