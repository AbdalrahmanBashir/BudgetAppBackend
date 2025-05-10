using BudgetAppBackend.Domain.PlaidTransactionAggregate.Entities;
using BudgetAppBackend.Domain.PlaidTransactionAggregate.ValueObjects;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BudgetAppBackend.Infrastructure.Configuration
{
    public class PlaidSyncCursorConfiguration : IEntityTypeConfiguration<PlaidSyncCursor>
    {
        public void Configure(EntityTypeBuilder<PlaidSyncCursor> builder)
        {
            
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id)
                .HasConversion(
                    id => id.Id,
                    value => PlaidSyncCursorId.Create(value))
                .IsRequired();

            builder.Property(c => c.UserId)
                .HasConversion(
                    id => id.Id,
                    value => UserId.Create(value))
                .IsRequired();

            builder.Property(c => c.ItemId);

            builder.Property(c => c.AccessToken)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(c => c.Cursor)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(c => c.LastSynced)
                .IsRequired();

            builder.Property(c => c.LastSyncStatus)
                .HasMaxLength(100);

            builder.HasIndex(c => new { c.UserId, c.AccessToken, c.ItemId })
                .IsUnique();
        }
    }
}
