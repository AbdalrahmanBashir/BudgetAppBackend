using BudgetAppBackend.Domain.PlaidTransactionAggregate;
using BudgetAppBackend.Domain.PlaidTransactionAggregate.ValueObjects;
using BudgetAppBackend.Domain.UserAggregate;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BudgetAppBackend.Infrastructure.Configuration
{
    public class PlaidTransactionConfiguration : IEntityTypeConfiguration<PlaidTransaction>
    {
        public void Configure(EntityTypeBuilder<PlaidTransaction> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasConversion(
                    id => id.Id,
                    value => PlaidTranId.Create(value));

            builder.Property(t => t.UserId)
                .HasConversion(
                    id => id.Id,
                    value => UserId.Create(value));

            builder.Property(t => t.PlaidTransactionId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.AccountId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Amount)
                .HasPrecision(19, 4)
                .IsRequired();

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(t => t.Date)
                .IsRequired();

            builder.Property<List<string>>("_categories")
                .HasColumnName("Categories")
                .HasConversion(
                    v => string.Join(";", v),
                    v => v.Split(';', StringSplitOptions.None).ToList()
                )
                .HasMaxLength(300);


            builder.Property(t => t.CategoryId)
                .HasMaxLength(100);

            builder.Property(t => t.MerchantName)
                .HasMaxLength(255);

            builder.Property(t => t.IsRemoved)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(t => t.CreatedAt)
                .IsRequired();

            builder.Property(t => t.LastModifiedAt)
                .IsRequired();

            builder.HasOne<User>()
                 .WithMany()
                 .HasForeignKey(t => t.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(t => t.PlaidTransactionId).IsUnique();
            builder.HasIndex(t => new { t.UserId, t.Date });
            builder.HasIndex(t => t.AccountId);
        }
    }
}