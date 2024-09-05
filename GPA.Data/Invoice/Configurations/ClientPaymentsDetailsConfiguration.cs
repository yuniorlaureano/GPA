using GPA.Common.Entities.Invoice;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.Invoice.Configurations
{
    public class ClientPaymentDetailsConfiguration : IEntityTypeConfiguration<ClientPaymentsDetails>
    {
        public void Configure(EntityTypeBuilder<ClientPaymentsDetails> builder)
        {
            builder.HasQueryFilter(x => !x.Deleted);

            builder.ToTable("ClientPaymentsDetails", GPASchema.INVOICE);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            builder.Property(x => x.Payment).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(x => x.PendingPayment).HasColumnType("decimal(18,2)").IsRequired();


            builder.HasOne(x => x.Invoice)
                .WithMany(x => x.ClientPaymentsDetails)
                .HasForeignKey(x => x.InvoiceId);
        }
    }
}
