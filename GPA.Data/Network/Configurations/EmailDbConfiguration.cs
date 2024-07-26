using GPA.Data.Schemas;
using GPA.Entities.Network;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.Network.Configurations
{
    public class EmailDbConfiguration : IEntityTypeConfiguration<EmailConfiguration>
    {
        public void Configure(EntityTypeBuilder<EmailConfiguration> builder)
        {
            builder.ToTable("EmailConfigurations", GPASchema.Network);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            builder.Property(x => x.Value).IsRequired();
            builder.Property(x => x.Provider).IsRequired();

        }
    }
}
