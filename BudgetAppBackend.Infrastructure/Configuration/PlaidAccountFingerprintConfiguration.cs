using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BudgetAppBackend.Domain.PlaidTransactionAggregate.Entities;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;

namespace BudgetAppBackend.Infrastructure.Configuration
{
    public class PlaidAccountFingerprintConfiguration : IEntityTypeConfiguration<PlaidAccountFingerprint>
    {
        public void Configure(EntityTypeBuilder<PlaidAccountFingerprint> builder)
        {
            builder.HasKey(e => new { e.UserId, e.ItemId });

            builder.Property(e => e.AccessToken)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Fingerprint)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(e => e.InstitutionName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.MaskedAccountNumbers)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasIndex(e => new { e.UserId, e.Fingerprint })
                .IsUnique();

            builder.Property(e => e.AccountIds)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());

            builder.Property(e => e.LastUpdated)
                .IsRequired();

            builder.Property(e => e.UserId)
                .HasConversion(
                    v => v.Id,
                    v => UserId.Create(v));
        }
    }
}