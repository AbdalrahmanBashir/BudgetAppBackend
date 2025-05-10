using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using BudgetAppBackend.Domain.UserAggregate;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore;

namespace BudgetAppBackend.Infrastructure.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            var userIdConverter = new ValueConverter<UserId, Guid>(
                v => v.Id,
                v => UserId.Create(v)
            );
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id)
                .HasColumnName("UserId")
                .ValueGeneratedNever()
                .HasConversion(userIdConverter!);

            builder.Property(u => u.FirstName)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(u => u.LastName)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(u => u.Email)
                .HasMaxLength(30)
                .IsRequired();
            builder.HasIndex(u => u.Email).IsUnique();

            builder.Property(u => u.PasswordHash);
            builder.Property(u => u.PasswordSalt);

            builder.Property(u => u.IsEmailVerified).HasDefaultValue(false);

            builder.Property(u => u.EmailVerificationCode).HasDefaultValue(null);
            builder.Property(u => u.EmailVerificationCodeExpiry).HasDefaultValue(null);

        }

    }

}
