using GPA.Data.Schemas;
using GPA.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.General.Configurations
{
    public class BlobStorageEFConfiguration : IEntityTypeConfiguration<BlobStorageConfiguration>
    {
        public void Configure(EntityTypeBuilder<BlobStorageConfiguration> builder)
        {
            builder.ToTable("BlobStorageConfigurations", GPASchema.GENERAL);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            builder.Property(x => x.Identifier).HasColumnType("NVARCHAR(50)").IsRequired();
            builder.Property(x => x.Provider).HasColumnType("NVARCHAR(50)").IsRequired();
            builder.Property(x => x.Value).HasColumnType("NVARCHAR(MAX)").IsRequired();
            builder.Property(x => x.Current).IsRequired();
        }
    }
}
