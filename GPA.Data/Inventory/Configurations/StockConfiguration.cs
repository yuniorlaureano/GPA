using GPA.Common.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace GPA.Data.Inventory.Configurations
{
    public class StockConfiguration : IEntityTypeConfiguration<Stock>
    {
        public void Configure(EntityTypeBuilder<Stock> builder)
        {
            builder.HasQueryFilter(x => !x.Deleted);

            builder.ToTable("Stocks", "Inventory");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasValueGenerator<SequentialGuidValueGenerator>()
                .IsRequired();

            builder.Property(x => x.Code).HasMaxLength(50);
            builder.Property(x => x.Input).IsRequired();
            builder.Property(x => x.OutInput).IsRequired();
            builder.Property(x => x.Deleted).IsRequired().HasDefaultValue(false);

            builder.HasOne(x => x.Provider)
                .WithMany(x => x.Stocks)
                .HasForeignKey(x => x.ProviderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Product)
                .WithMany(x => x.Stocks)
                .HasForeignKey(x => x.ProductId)
                .IsRequired();
        }
    }
}
