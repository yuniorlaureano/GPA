using GPA.Data.Schemas;
using GPA.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.Inventory.Configurations
{
    public class StockAttachmentConfiguration : IEntityTypeConfiguration<StockAttachment>
    {
        public void Configure(EntityTypeBuilder<StockAttachment> builder)
        {
            builder.ToTable("StockAttachments", GPASchema.INVENTORY);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            builder.Property(x => x.File).IsRequired();

            builder.HasOne(x => x.Stock)
                .WithMany(x => x.StockAttachments)
                .HasForeignKey(x => x.StockId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
