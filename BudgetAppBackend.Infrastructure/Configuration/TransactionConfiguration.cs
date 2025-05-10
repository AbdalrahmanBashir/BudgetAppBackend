using BudgetAppBackend.Domain.TransactionAggregate;
using BudgetAppBackend.Domain.TransactionAggregate.ValueObjects;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BudgetAppBackend.Domain.UserAggregate;

namespace BudgetAppBackend.Infrastructure.Configuration
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasConversion(
                    id => id.Id,
                    value => TransactionId.Create(value)
                )
                .ValueGeneratedNever();

            builder.Property(t => t.UserId)
                .HasConversion(
                    userId => userId.Id,
                    value => UserId.Create(value)
                )
                .IsRequired();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property<List<string>>("_categories")
                .HasColumnName("Categories")
                .HasConversion(
                    v => string.Join(";", v),
                    v => v.Split(';', StringSplitOptions.None).ToList()
                )
                .HasMaxLength(300);

            builder.Property(t => t.Amount)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(t => t.TransactionDate)
                .IsRequired();

            builder.Property(t => t.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(t => t.Payee)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(t => t.Type)
                .HasConversion<string>()
                .IsRequired();

            builder.HasIndex(t => t.UserId);
            builder.HasIndex(t => t.CreatedDate);
            builder.HasIndex(t => t.Type);

            builder.ToTable("Transactions");
        }
    }
}
