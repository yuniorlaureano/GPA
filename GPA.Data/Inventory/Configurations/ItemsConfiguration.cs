using GPA.Common.Entities.Inventory;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace GPA.Data.Inventory.Configurations
{
    public class ItemsConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.HasQueryFilter(x => !x.Deleted);

            builder.ToTable("Items", GPASchema.INVENTORY);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasValueGenerator<SequentialGuidValueGenerator>()
                .IsRequired();
        }
    }
}
