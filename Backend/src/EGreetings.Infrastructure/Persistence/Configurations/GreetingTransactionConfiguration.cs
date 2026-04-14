using EGreetings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EGreetings.Infrastructure.Persistence.Configurations;

public class GreetingTransactionConfiguration : IEntityTypeConfiguration<GreetingTransaction>
{
    public void Configure(EntityTypeBuilder<GreetingTransaction> builder)
    {
        builder.HasKey(gt => gt.Id);

        builder.Property(gt => gt.RecipientEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(gt => gt.ErrorMessage)
            .HasMaxLength(1000);

        builder.HasOne(gt => gt.Greeting)
            .WithMany(g => g.Transactions)
            .HasForeignKey(gt => gt.GreetingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
