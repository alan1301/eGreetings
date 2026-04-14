using EGreetings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EGreetings.Infrastructure.Persistence.Configurations;

public class DraftConfiguration : IEntityTypeConfiguration<Draft>
{
    public void Configure(EntityTypeBuilder<Draft> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.RecipientEmail)
            .HasMaxLength(255);

        builder.Property(d => d.RecipientName)
            .HasMaxLength(150);

        builder.Property(d => d.SenderMessage)
            .HasMaxLength(2000);

        builder.HasOne(d => d.User)
            .WithMany(u => u.Drafts)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Template)
            .WithMany()
            .HasForeignKey(d => d.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
