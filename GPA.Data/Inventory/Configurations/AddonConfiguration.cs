using GPA.Common.Entities.Inventory;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.Inventory.Configurations
{
    public class AddonConfiguration : IEntityTypeConfiguration<Addon>
    {
        public void Configure(EntityTypeBuilder<Addon> builder)
        {
            builder.ToTable("Addons", GPASchema.INVENTORY);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            builder.Property(x => x.Concept).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Value).HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(x => x.Type).IsRequired();

            builder.HasMany(x => x.ProductAddons)
                .WithOne(x => x.Addon)
                .HasForeignKey(x => x.AddonId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
