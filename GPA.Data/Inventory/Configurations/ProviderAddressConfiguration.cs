using GPA.Common.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace GPA.Data.Inventory.Configurations
{
    public class ProviderAddressConfiguration : IEntityTypeConfiguration<ProviderAddress>
    {
        public void Configure(EntityTypeBuilder<ProviderAddress> builder)
        {
            builder.HasQueryFilter(x => !x.Deleted);

            builder.ToTable("ProviderAddresses", "Inventory");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasValueGenerator<SequentialGuidValueGenerator>()
                .IsRequired();

            builder.Property(x => x.Street).HasMaxLength(50);
            builder.Property(x => x.BuildingNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.City).HasMaxLength(100).IsRequired();
            builder.Property(x => x.State).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Country).HasMaxLength(100);
            builder.Property(x => x.PostalCode).HasMaxLength(100);
            builder.Property(x => x.Deleted).IsRequired().HasDefaultValue(false);

            builder.HasOne(x => x.Provider)
                .WithMany(x => x.ProviderAddresses)
                .HasForeignKey(x => x.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
