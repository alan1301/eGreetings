using EGreetings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EGreetings.Infrastructure.Persistence.Configurations;

public class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
{
    public void Configure(EntityTypeBuilder<EmailLog> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ToEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(e => e.Status)
            .HasConversion<string>();

        builder.HasOne(e => e.Greeting)
            .WithMany(g => g.EmailLogs)
            .HasForeignKey(e => e.GreetingId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
