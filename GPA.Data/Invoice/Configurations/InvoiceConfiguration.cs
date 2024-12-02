using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.Invoice.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<GPA.Common.Entities.Invoice.Invoice>
    {
        public void Configure(EntityTypeBuilder<GPA.Common.Entities.Invoice.Invoice> builder)
        {
            builder.ToTable("Invoices", GPASchema.INVOICE);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            builder.Property(x => x.Payment).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(x => x.ToPay).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Date).IsRequired();
            builder.Property(x => x.Note).HasMaxLength(300);
            builder.Property(x => x.Type).IsRequired();
            builder.Property(x => x.Status).IsRequired();
            builder.Property(x => x.PaymentStatus).IsRequired();
            builder.Property(x => x.PaymentMethod).IsRequired();

            builder.HasMany(x => x.InvoiceAttachments)
                .WithOne(x => x.Invoice)
                .HasForeignKey(x => x.InvoiceId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(x => x.ClientPaymentsDetails)
                .WithOne(x => x.Invoice)
                .HasForeignKey(x => x.InvoiceId);

            builder.HasOne(x => x.Client)
                .WithMany(x => x.Invoices)
                .HasForeignKey(x => x.ClientId);

            builder.HasMany(x => x.InvoiceDetails)
                .WithOne(x => x.Invoice)
                .HasForeignKey(x => x.InvoiceId)
                .IsRequired();

        }
    }
}

