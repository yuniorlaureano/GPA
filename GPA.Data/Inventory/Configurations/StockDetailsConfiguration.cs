using GPA.Common.Entities.Inventory;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.Inventory.Configurations
{
    public class StockDetailsConfiguration : IEntityTypeConfiguration<StockDetails>
    {
        public void Configure(EntityTypeBuilder<StockDetails> builder)
        {
            builder.ToTable("StockDetails", GPASchema.INVENTORY);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            builder.Property(x => x.Deleted).IsRequired().HasDefaultValue(false);

            builder.HasOne(x => x.Product)
               .WithMany(x => x.Stocks)
               .HasForeignKey(x => x.ProductId)
               .IsRequired();
        }
    }
}
