using GPA.Common.Entities.Invoice;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace GPA.Data.Invoice.Configurations
{
    public class ClientAddressConfiguration : IEntityTypeConfiguration<ClientAddress>
    {
        public void Configure(EntityTypeBuilder<ClientAddress> builder)
        {
            builder.HasQueryFilter(x => !x.Deleted);

            builder.ToTable("ClientAddresses", GPASchema.INVOICE);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasValueGenerator<SequentialGuidValueGenerator>()
                .IsRequired();

            builder.HasOne(x => x.Client)
                .WithMany(x => x.ClientAddresses)
                .HasForeignKey(x => x.ClientId);
        }
    }
}
