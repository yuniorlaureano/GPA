using GPA.Common.Entities.Inventory;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.Inventory.Configurations
{
    public class ProductAddonConfiguration : IEntityTypeConfiguration<ProductAddon>
    {
        public void Configure(EntityTypeBuilder<ProductAddon> builder)
        {
            builder.HasQueryFilter(x => !x.Deleted);

            builder.ToTable("ProductAddons", GPASchema.INVENTORY);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            builder.HasOne(x => x.Addon)
                .WithMany(x => x.ProductAddons)
                .HasForeignKey(x => x.AddonId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Product)
                .WithMany(x => x.ProductAddons)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

        }
    }
}
