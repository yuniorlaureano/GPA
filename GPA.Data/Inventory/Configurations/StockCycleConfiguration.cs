using GPA.Common.Entities.Inventory;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.Inventory.Configurations
{
    public class StockCycleConfiguration : IEntityTypeConfiguration<StockCycle>
    {
        public void Configure(EntityTypeBuilder<StockCycle> builder)
        {
            builder.HasQueryFilter(x => !x.Deleted);

            builder.ToTable("StockCycles", GPASchema.INVENTORY);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            builder.Property(x => x.Deleted).IsRequired().HasDefaultValue(false);

            builder.HasMany(x => x.StockCycleDetails)
                .WithOne(x => x.StockCycle)
                .HasForeignKey(x => x.StockCycleId)
                .IsRequired();
        }
    }
}
