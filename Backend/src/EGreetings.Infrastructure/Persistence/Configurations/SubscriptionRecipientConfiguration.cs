using EGreetings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EGreetings.Infrastructure.Persistence.Configurations;

public class SubscriptionRecipientConfiguration : IEntityTypeConfiguration<SubscriptionRecipient>
{
    public void Configure(EntityTypeBuilder<SubscriptionRecipient> builder)
    {
        builder.HasKey(sr => sr.Id);

        builder.Property(sr => sr.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(sr => sr.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(sr => sr.Occasion)
            .HasMaxLength(100);

        builder.HasOne(sr => sr.Subscription)
            .WithMany(s => s.Recipients)
            .HasForeignKey(sr => sr.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
