using GPA.Common.Entities.Inventory;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace GPA.Data.Inventory.Configurations
{
    public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
    {
        public void Configure(EntityTypeBuilder<Provider> builder)
        {
            builder.ToTable("Providers", GPASchema.INVENTORY);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasValueGenerator<SequentialGuidValueGenerator>()
                .IsRequired();

            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.Property(x => x.LastName).HasMaxLength(100);
            builder.Property(x => x.Phone).HasMaxLength(15).IsRequired();
            builder.Property(x => x.Email).HasMaxLength(254).IsRequired();
            builder.Property(x => x.Deleted).IsRequired().HasDefaultValue(false);
            builder.Property(x => x.Identification).IsRequired().HasMaxLength(15);
            builder.Property(x => x.IdentificationType).IsRequired();

            builder.Property(x => x.Street).HasMaxLength(100);
            builder.Property(x => x.City).HasMaxLength(50);
            builder.Property(x => x.State).HasMaxLength(50);
            builder.Property(x => x.Country).HasMaxLength(50);
            builder.Property(x => x.PostalCode).HasMaxLength(50);
            builder.Property(x => x.BuildingNumber).HasMaxLength(10);
            builder.Property(x => x.FormattedAddress).HasMaxLength(256);

            builder.HasMany(x => x.Stocks)
                .WithOne(x => x.Provider)
                .HasForeignKey(x => x.ProviderId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
