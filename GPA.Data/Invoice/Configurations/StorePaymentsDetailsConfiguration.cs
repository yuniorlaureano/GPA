using GPA.Common.Entities.Invoice;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.Invoice.Configurations
{
    public class StorePaymentsDetailsConfiguration : IEntityTypeConfiguration<StorePaymentsDetails>
    {
        public void Configure(EntityTypeBuilder<StorePaymentsDetails> builder)
        {
            builder.HasQueryFilter(x => !x.Deleted);

            builder.ToTable("StorePaymentsDetails", GPASchema.INVOICE);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();
            builder.Property(x => x.Payment).HasColumnType("decimal(18,2)").IsRequired();
        }
    }
}
