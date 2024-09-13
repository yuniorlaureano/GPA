using GPA.Data.Schemas;
using GPA.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.General.Configurations
{
    public class PrintInformationConfiguration : IEntityTypeConfiguration<PrintInformation>
    {
        public void Configure(EntityTypeBuilder<PrintInformation> builder)
        {
            builder.ToTable("PrintInformation", GPASchema.GENERAL);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            builder.Property(x => x.CompanyLogo).IsRequired(false);
            builder.Property(x => x.CompanyName).HasMaxLength(254).IsRequired();
            builder.Property(x => x.CompanyDocument).HasMaxLength(30).IsRequired();
            builder.Property(x => x.CompanyDocumentPrefix).HasMaxLength(10).IsRequired();
            builder.Property(x => x.CompanyAddress).HasMaxLength(254).IsRequired();
            builder.Property(x => x.CompanyPhone).HasMaxLength(30).IsRequired();
            builder.Property(x => x.CompanyPhonePrefix).HasMaxLength(10).IsRequired();
            builder.Property(x => x.CompanyEmail).HasMaxLength(254).IsRequired();
            builder.Property(x => x.Signer).HasMaxLength(100).IsRequired();
            builder.Property(x => x.CompanyWebsite).IsRequired();
            builder.Property(x => x.Current).IsRequired();

        }
    }
}
