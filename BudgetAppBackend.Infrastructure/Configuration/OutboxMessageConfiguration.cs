using BudgetAppBackend.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BudgetAppBackend.Infrastructure.Configuration
{
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("OutboxMessages");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                   .HasColumnName("OutboxMessageId")
                   .ValueGeneratedNever();

            builder.Property(o => o.AggregateId)
                   .IsRequired();

            builder.Property(o => o.EventType)
                   .HasMaxLength(256)
                   .IsRequired();

            builder.Property(o => o.Payload)
                   .IsRequired();

            builder.Property(o => o.CreatedAt)
                   .IsRequired();

            builder.Property(o => o.ProcessedAt)
                   .IsRequired(false);

            builder.HasIndex(o => o.CreatedAt).HasDatabaseName("IX_OutboxMessages_CreatedAt");
        }
    }
}
