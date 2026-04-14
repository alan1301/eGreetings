using EGreetings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EGreetings.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.Currency).HasMaxLength(10).HasDefaultValue("VND");
        builder.Property(p => p.TransactionCode).IsRequired().HasMaxLength(50);
        builder.HasIndex(p => p.TransactionCode).IsUnique();
        builder.Property(p => p.PaymentMethod).HasMaxLength(50);
        builder.Property(p => p.PaymentGatewayRef).HasMaxLength(200);
        builder.Property(p => p.GuestEmail).HasMaxLength(200);
        builder.Property(p => p.Status).HasConversion<int>();

        builder.HasOne(p => p.User)
            .WithMany(u => u.Payments)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
