using EGreetings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EGreetings.Infrastructure.Persistence.Configurations;

public class GreetingTemplateConfiguration : IEntityTypeConfiguration<GreetingTemplate>
{
    public void Configure(EntityTypeBuilder<GreetingTemplate> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.ThumbnailUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.HtmlContent)
            .IsRequired();

        builder.Property(t => t.CssStyle)
            .HasDefaultValue(string.Empty);

        builder.HasOne(t => t.Category)
            .WithMany(c => c.Templates)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Greetings)
            .WithOne(g => g.Template)
            .HasForeignKey(g => g.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
