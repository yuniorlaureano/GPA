using GPA.Common.Entities.Inventory;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.Inventory.Configurations
{
    public class StockCycleDetailConfiguration : IEntityTypeConfiguration<StockCycleDetail>
    {
        public void Configure(EntityTypeBuilder<StockCycleDetail> builder)
        {
            builder.ToTable("StockCycleDetails", GPASchema.INVENTORY);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            builder.HasOne(x => x.StockCycle)
                .WithMany(x => x.StockCycleDetails)
                .HasForeignKey(x => x.StockCycleId)
                .IsRequired();
        }
    }
}
