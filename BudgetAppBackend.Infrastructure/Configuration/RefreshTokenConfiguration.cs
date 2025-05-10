using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BudgetAppBackend.Domain.UserAggregate.Entities;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using BudgetAppBackend.Domain.UserAggregate;

namespace BudgetAppBackend.Infrastructure.Configuration
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.UserId)
                .HasConversion(
                    userId => userId.Id,
                    value => UserId.Create(value)
                )
                .HasColumnName("UserId")
                .IsRequired();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey("UserId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(rt => rt.UserId);

            builder.HasIndex(rt => rt.TokenHash).IsUnique();
            builder.Property(rt => rt.TokenHash)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(rt => rt.ExpiryDate)
                .IsRequired();

            builder.Property(rt => rt.CreatedAt)
                .IsRequired();

            builder.Property(rt => rt.RevokedAt)
                .IsRequired(false);
        }
    }
}
