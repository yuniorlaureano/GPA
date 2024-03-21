using GPA.Common.Entities.Invoice;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace GPA.Data.Invoice.Configurations
{
    public class SellDetailsConfiguration : IEntityTypeConfiguration<SellDetails>
    {
        public void Configure(EntityTypeBuilder<SellDetails> builder)
        {
            builder.HasQueryFilter(x => !x.Deleted);

            builder.ToTable("SellDetails", GPASchema.INVOICE);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasValueGenerator<SequentialGuidValueGenerator>()
                .IsRequired();

        }
    }
}
