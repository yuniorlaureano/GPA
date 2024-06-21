using GPA.Common.Entities.Invoice;
using GPA.Data.Schemas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.Invoice.Configurations
{
    public class ClientCreditConfiguration : IEntityTypeConfiguration<ClientCredit>
    {
        public void Configure(EntityTypeBuilder<ClientCredit> builder)
        {
            builder.HasQueryFilter(x => !x.Deleted);

            builder.ToTable("ClientCredits", GPASchema.INVOICE);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            builder.HasOne(x => x.Client)
                .WithMany(x => x.Credits)
                .HasForeignKey(x => x.ClientId);
        }
    }
}
