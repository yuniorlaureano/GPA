using GPA.Common.Entities.Security;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.Security.Configurations
{
    public class ProfileConfigurations : IEntityTypeConfiguration<GPAProfile>
    {
        public void Configure(EntityTypeBuilder<GPAProfile> builder)
        {
            builder.ToTable("Profiles", GPASchema.SECURITY);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            builder.Property(b => b.Name).IsRequired().HasMaxLength(50);
            builder.HasIndex(b => b.Name).IsUnique();

            builder.HasMany(p => p.Users) 
                .WithOne(p => p.Profile)
                .HasForeignKey(p => p.ProfileId);
        }
    }
}
