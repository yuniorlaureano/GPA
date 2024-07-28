using GPA.Data.Schemas;
using GPA.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.General.Configurations
{
    public class EmailDbConfiguration : IEntityTypeConfiguration<EmailConfiguration>
    {
        public void Configure(EntityTypeBuilder<EmailConfiguration> builder)
        {
            builder.ToTable("EmailConfigurations", GPASchema.GENERAL);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            builder.Property(x => x.Identifier).HasColumnType("NVARCHAR(50)").IsRequired();
            builder.Property(x => x.Engine).HasColumnType("NVARCHAR(50)").IsRequired();
            builder.Property(x => x.Value).HasColumnType("NVARCHAR(MAX)").IsRequired();
            builder.Property(x => x.From).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Current).IsRequired();
        }
    }
}
