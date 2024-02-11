using GPA.Common.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace GPA.Data.Inventory.Configurations
{
    public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
    {
        public void Configure(EntityTypeBuilder<Provider> builder)
        {
            builder.HasQueryFilter(x => !x.Deleted);

            builder.ToTable("Providers", "Inventory");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasValueGenerator<SequentialGuidValueGenerator>()
                .IsRequired();

            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.Property(x => x.RNC).HasMaxLength(15);
            builder.Property(x => x.Phone).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Email).HasMaxLength(30).IsRequired();
            builder.Property(x => x.Deleted).IsRequired().HasDefaultValue(false);

            builder.HasMany(x => x.Stocks)
                .WithOne(x => x.Provider)
                .HasForeignKey(x => x.ProviderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(x => x.ProviderAddresses)
                .WithOne(x => x.Provider)
                .HasForeignKey(x => x.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
