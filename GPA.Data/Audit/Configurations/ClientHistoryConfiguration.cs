﻿using GPA.Data.Schemas;
using GPA.Entities.Unmapped.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPA.Data.Audit.Configurations
{
    public class ClientHistoryConfiguration : IEntityTypeConfiguration<ClientHistory>
    {
        public void Configure(EntityTypeBuilder<ClientHistory> builder)
        {
            builder.ToTable("ClientHistory", GPASchema.AUDIT);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()")
                .IsRequired();

            builder.Property(x => x.Action).HasMaxLength(50).IsRequired();
        }
    }
}
