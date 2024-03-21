using GPA.Common.Entities.Invoice;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace GPA.Data.Invoice.Configurations
{
    public class ClientPaymentDetailsConfiguration : IEntityTypeConfiguration<ClientPaymentsDetails>
    {
        public void Configure(EntityTypeBuilder<ClientPaymentsDetails> builder)
        {
            builder.HasQueryFilter(x => !x.Deleted);

            builder.ToTable("ClientPaymentsDetails", GPASchema.INVOICE);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasValueGenerator<SequentialGuidValueGenerator>()
                .IsRequired();

            builder.HasOne(x => x.Invoice)
                .WithMany(x => x.ClientPaymentsDetails)
                .HasForeignKey(x => x.InvoiceId);
        }
    }
}
