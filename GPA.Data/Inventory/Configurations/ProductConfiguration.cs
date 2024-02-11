using GPA.Common.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace GPA.Data.Inventory.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasQueryFilter(x => !x.Deleted);

            builder.ToTable("Products", "Inventory");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasValueGenerator<SequentialGuidValueGenerator>();
            builder.Property(x => x.Code).HasMaxLength(50);
            builder.Property(x => x.Name).HasMaxLength(200);
            builder.Property(x => x.Photo).HasMaxLength(300);
            builder.Property(x => x.UnitCost).HasPrecision(15,2);
            builder.Property(x => x.WholesaleCost).HasPrecision(15,2);
            builder.Property(x => x.Description).HasMaxLength(300);

            builder.HasOne(x => x.Category)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ProductLocation)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.ProductLocationId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
