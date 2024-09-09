using GPA.Data.Schemas;
using GPA.Entities.Unmapped.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.Audit.Configurations
{
    public class AddonHistoryConfiguration : IEntityTypeConfiguration<AddonHistory>
    {
        public void Configure(EntityTypeBuilder<AddonHistory> builder)
        {
            builder.ToTable("AddonHistory", GPASchema.AUDIT);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            builder.Property(x => x.Concept).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Value).HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(x => x.Action).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Type).IsRequired();
        }
    }
}
