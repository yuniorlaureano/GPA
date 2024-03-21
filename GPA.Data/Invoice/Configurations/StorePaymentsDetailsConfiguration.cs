using GPA.Common.Entities.Invoice;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace GPA.Data.Invoice.Configurations
{
    public class StorePaymentsDetailsConfiguration : IEntityTypeConfiguration<StorePaymentsDetails>
    {
        public void Configure(EntityTypeBuilder<StorePaymentsDetails> builder)
        {
            builder.HasQueryFilter(x => !x.Deleted);

            builder.ToTable("StorePaymentsDetails", GPASchema.INVOICE);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasValueGenerator<SequentialGuidValueGenerator>()
                .IsRequired();
        }
    }
}
