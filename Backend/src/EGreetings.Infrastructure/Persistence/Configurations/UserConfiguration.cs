using EGreetings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EGreetings.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.Phone).HasMaxLength(20);
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);
        builder.Property(u => u.EmailVerificationToken).HasMaxLength(64);
        builder.Property(u => u.PasswordResetToken).HasMaxLength(64);
        builder.Property(u => u.Role).HasConversion<int>();
        builder.Property(u => u.Status).HasConversion<int>();

        // BR-04: Login lockout
        builder.Property(u => u.FailedLoginAttempts).HasDefaultValue(0);
    }
}
