using EGreetings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EGreetings.Infrastructure.Persistence.Configurations;

public class GreetingConfiguration : IEntityTypeConfiguration<Greeting>
{
    public void Configure(EntityTypeBuilder<Greeting> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.RecipientEmail).IsRequired().HasMaxLength(200);
        builder.Property(g => g.RecipientName).IsRequired().HasMaxLength(100);
        builder.Property(g => g.SenderMessage).HasMaxLength(2000);
        builder.Property(g => g.UniqueToken).IsRequired().HasMaxLength(64);
        builder.HasIndex(g => g.UniqueToken).IsUnique();
        builder.Property(g => g.GuestSenderEmail).HasMaxLength(200);
        builder.Property(g => g.GuestSenderName).HasMaxLength(100);
        builder.Property(g => g.ReplyToEmail).HasMaxLength(200);
        builder.Property(g => g.Status).HasConversion<int>();

        builder.HasOne(g => g.User)
            .WithMany(u => u.Greetings)
            .HasForeignKey(g => g.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(g => g.Template)
            .WithMany(t => t.Greetings)
            .HasForeignKey(g => g.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
