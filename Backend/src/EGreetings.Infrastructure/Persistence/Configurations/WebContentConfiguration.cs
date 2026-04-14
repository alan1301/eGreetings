using EGreetings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EGreetings.Infrastructure.Persistence.Configurations;

public class WebContentConfiguration : IEntityTypeConfiguration<WebContent>
{
    public void Configure(EntityTypeBuilder<WebContent> builder)
    {
        builder.HasKey(wc => wc.Id);

        builder.Property(wc => wc.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(wc => wc.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(wc => wc.ImageUrl)
            .HasMaxLength(500);

        // Self-referencing for version history
        builder.HasOne(wc => wc.PreviousVersion)
            .WithMany()
            .HasForeignKey(wc => wc.PreviousVersionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(wc => wc.UpdatedByUser)
            .WithMany()
            .HasForeignKey(wc => wc.UpdatedByUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
