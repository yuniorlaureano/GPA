using GPA.Data.Schemas;
using GPA.Entities.Invoice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.Invoice.Configurations
{
    public class InvoicePrintConfigurationConfiguration : IEntityTypeConfiguration<InvoicePrintConfiguration>
    {
        public void Configure(EntityTypeBuilder<InvoicePrintConfiguration> builder)
        {
            builder.ToTable("InvoicePrintConfigurations", GPASchema.GENERAL);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            builder.Property(x => x.CompanyLogo).HasMaxLength(254).IsRequired(false);
            builder.Property(x => x.CompanyName).HasMaxLength(254).IsRequired();
            builder.Property(x => x.CompanyDocument).HasMaxLength(30).IsRequired();
            builder.Property(x => x.CompanyAddress).HasMaxLength(254).IsRequired();
            builder.Property(x => x.CompanyPhone).HasMaxLength(30).IsRequired();
            builder.Property(x => x.CompanyEmail).HasMaxLength(254).IsRequired();
            builder.Property(x => x.Signer).HasMaxLength(100).IsRequired();
            builder.Property(x => x.CompanyWebsite).IsRequired();
            builder.Property(x => x.Current).IsRequired();

        }
    }
}
