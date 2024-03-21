using GPA.Data.Schemas;
using GPA.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace GPA.Data.Common.Configurations
{
    public class UnitConfiguration : IEntityTypeConfiguration<Unit>
    {
        public void Configure(EntityTypeBuilder<Unit> builder)
        {
            builder.HasQueryFilter(x => !x.Deleted);

            builder.ToTable("Units", GPASchema.COMMON);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasValueGenerator<SequentialGuidValueGenerator>()
                .IsRequired();
        }
    }
}
