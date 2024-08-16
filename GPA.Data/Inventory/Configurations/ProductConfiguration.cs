using GPA.Common.Entities.Inventory;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.Inventory.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasQueryFilter(x => !x.Deleted);

            builder.ToTable("Products", GPASchema.INVENTORY);
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            builder.Property(x => x.Code).HasMaxLength(50);
            builder.Property(x => x.Name).HasMaxLength(200);
            builder.Property(x => x.Photo);
            builder.Property(x => x.Price).HasColumnType("decimal(10,2)");
            builder.Property(x => x.Description).HasMaxLength(300);

            builder.HasOne(x => x.Category)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ProductLocation)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.ProductLocationId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(x => x.ProductAddons)
                .WithOne(x => x.Product)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
