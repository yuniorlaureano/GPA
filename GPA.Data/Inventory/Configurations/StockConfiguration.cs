using GPA.Common.Entities.Inventory;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.Inventory.Configurations
{
    public class StockConfiguration : IEntityTypeConfiguration<Stock>
    {
        public void Configure(EntityTypeBuilder<Stock> builder)
        {
            builder.HasQueryFilter(x => !x.Deleted);

            builder.ToTable("Stocks", GPASchema.INVENTORY);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            builder.Property(x => x.Deleted).IsRequired().HasDefaultValue(false);

            builder.HasOne(x => x.Provider)
                .WithMany(x => x.Stocks)
                .HasForeignKey(x => x.ProviderId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(x => x.StockDetails)
                .WithOne(x => x.Stock)
                .HasForeignKey(x => x.StockId)
                .IsRequired();

            builder.HasOne(x => x.Reason)
               .WithMany(x => x.Stocks)
               .HasForeignKey(x => x.ReasonId)
               .IsRequired();
        }
    }
}
