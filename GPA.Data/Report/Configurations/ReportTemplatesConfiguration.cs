using GPA.Data.Schemas;
using GPA.Entities.Report;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.General.Configurations
{
    public class ReportTemplatesConfiguration : IEntityTypeConfiguration<ReportTemplate>
    {
        public void Configure(EntityTypeBuilder<ReportTemplate> builder)
        {
            builder.ToTable("ReportTemplates", GPASchema.GENERAL);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            builder.Property(x => x.Code).HasColumnType("NVARCHAR(50)").IsRequired();
            builder.Property(x => x.Template).HasColumnType("NVARCHAR(MAX)").IsRequired();
        }
    }
}
