using GPA.Common.Entities.Inventory;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace GPA.Data.Inventory.Configurations
{
    public class ProductLocationConfiguration : IEntityTypeConfiguration<ProductLocation>
    {
        public void Configure(EntityTypeBuilder<ProductLocation> builder)
        {
            builder.HasQueryFilter(x => !x.Deleted);

            builder.ToTable("ProductLocations", GPASchema.INVENTORY);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasValueGenerator<SequentialGuidValueGenerator>()
                .IsRequired();

            builder.Property(x => x.Code).HasMaxLength(20);
            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(200);
            builder.Property(x => x.Deleted).IsRequired().HasDefaultValue(false);

            builder.HasMany(x => x.Products)
                .WithOne(x => x.ProductLocation)
                .HasForeignKey(x => x.ProductLocationId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
