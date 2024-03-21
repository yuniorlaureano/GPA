using GPA.Common.Entities.Invoice;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace GPA.Data.Invoice.Configurations
{
    public class SellConfiguration : IEntityTypeConfiguration<Sell>
    {
        public void Configure(EntityTypeBuilder<Sell> builder)
        {
            builder.HasQueryFilter(x => !x.Deleted);

            builder.ToTable("Sells", GPASchema.INVOICE);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasValueGenerator<SequentialGuidValueGenerator>()
                .IsRequired();

            builder.HasOne(x => x.Invoice)
                .WithOne(x => x.Sell)
                .HasForeignKey<GPA.Common.Entities.Invoice.Invoice>(x => x.SellId);

        }
    }
}
