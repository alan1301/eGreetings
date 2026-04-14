using EGreetings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EGreetings.Infrastructure.Persistence.Configurations;

public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Subject)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.Content)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(f => f.ContactEmail)
            .HasMaxLength(255);

        builder.Property(f => f.StarRating)
            .IsRequired(false);    // UC07: 1–5, nullable

        builder.Property(f => f.AdminReply)
            .HasMaxLength(2000);

        builder.HasOne(f => f.User)
            .WithMany(u => u.Feedbacks)
            .HasForeignKey(f => f.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
