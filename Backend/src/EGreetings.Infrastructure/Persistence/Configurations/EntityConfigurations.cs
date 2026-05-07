using EGreetings.Domain.Entities;
using EGreetings.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EGreetings.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("Users");
        b.HasKey(x => x.Id);

        // BR-02: Email must be unique
        b.Property(x => x.Email).IsRequired().HasMaxLength(256).IsUnicode(false);
        b.HasIndex(x => x.Email).IsUnique().HasDatabaseName("UQ_Users_Email");

        b.Property(x => x.FullName).IsRequired().HasMaxLength(100);
        b.Property(x => x.PasswordHash).HasMaxLength(256);
        b.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.SocialProvider).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.SocialProviderId).HasMaxLength(200);
        b.Property(x => x.EmailVerificationToken).HasMaxLength(64);
        b.Property(x => x.PasswordResetToken).HasMaxLength(64);
        b.Property(x => x.RefreshTokenHash).HasMaxLength(256);

        // Soft delete global filter
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.ToTable("Categories");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(100);
        b.Property(x => x.Slug).IsRequired().HasMaxLength(120).IsUnicode(false);
        b.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("UQ_Categories_Slug");
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.IconUrl).HasMaxLength(500);
        b.Property(x => x.Color).HasMaxLength(20);
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class GreetingCardConfiguration : IEntityTypeConfiguration<GreetingCard>
{
    public void Configure(EntityTypeBuilder<GreetingCard> b)
    {
        b.ToTable("GreetingCards");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(200);
        b.Property(x => x.Slug).IsRequired().HasMaxLength(220).IsUnicode(false);
        b.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("UQ_GreetingCards_Slug");
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.ThumbnailUrl).HasMaxLength(1000);
        b.Property(x => x.FileUrl).HasMaxLength(1000);
        b.Property(x => x.Tags).HasMaxLength(500);
        b.Property(x => x.CustomJsonContent);

        b.HasOne(x => x.Category)
            .WithMany(c => c.Cards)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class DraftConfiguration : IEntityTypeConfiguration<Draft>
{
    public void Configure(EntityTypeBuilder<Draft> b)
    {
        b.ToTable("Drafts");
        b.HasKey(x => x.Id);
        // BR-08: PersonalMessage max 500 chars
        b.Property(x => x.PersonalMessage).HasMaxLength(500);
        b.Property(x => x.CustomJsonContent);

        b.HasOne(x => x.User).WithMany(u => u.Drafts)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Card).WithMany(c => c.Drafts)
            .HasForeignKey(x => x.CardId).OnDelete(DeleteBehavior.Restrict);

        b.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class GreetingTransactionConfiguration : IEntityTypeConfiguration<GreetingTransaction>
{
    public void Configure(EntityTypeBuilder<GreetingTransaction> b)
    {
        b.ToTable("GreetingTransactions");
        b.HasKey(x => x.Id);
        // BR-12: NEVER delete – no soft delete, no global filter
        b.Property(x => x.RecipientEmail).IsRequired().HasMaxLength(256).IsUnicode(false);
        b.HasIndex(x => x.RecipientEmail).HasDatabaseName("IX_GreetingTransactions_RecipientEmail");
        b.Property(x => x.Subject).IsRequired().HasMaxLength(300);
        b.Property(x => x.PersonalMessage).HasMaxLength(500);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        b.HasOne(x => x.Sender).WithMany(u => u.SentTransactions)
            .HasForeignKey(x => x.SenderId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Card).WithMany(c => c.Transactions)
            .HasForeignKey(x => x.CardId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Subscription).WithMany(s => s.GreetingTransactions)
            .HasForeignKey(x => x.SubscriptionId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
    }
}

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> b)
    {
        b.ToTable("Subscriptions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.DisabledReason).HasMaxLength(500);

        b.HasIndex(x => new { x.UserId, x.Status }).HasDatabaseName("IX_Subscriptions_UserId_Status");

        b.HasOne(x => x.User).WithMany(u => u.Subscriptions)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SubscriptionEmailListConfiguration : IEntityTypeConfiguration<SubscriptionEmailList>
{
    public void Configure(EntityTypeBuilder<SubscriptionEmailList> b)
    {
        b.ToTable("SubscriptionEmailLists");
        b.HasKey(x => x.Id);
        b.Property(x => x.Email).IsRequired().HasMaxLength(256).IsUnicode(false);

        b.HasOne(x => x.Subscription).WithMany(s => s.EmailList)
            .HasForeignKey(x => x.SubscriptionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> b)
    {
        b.ToTable("PaymentTransactions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        b.Property(x => x.Currency).HasMaxLength(10);
        b.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.GatewayTransactionId).HasMaxLength(200);
        b.Property(x => x.GatewayProvider).HasMaxLength(50);

        b.HasOne(x => x.Subscription).WithMany(s => s.PaymentTransactions)
            .HasForeignKey(x => x.SubscriptionId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> b)
    {
        b.ToTable("Contacts");
        b.HasKey(x => x.Id);
        // BR-21
        b.Property(x => x.Name).IsRequired().HasMaxLength(100);
        b.Property(x => x.Email).IsRequired().HasMaxLength(256).IsUnicode(false);
        b.Property(x => x.Group).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.OccasionLabel).HasMaxLength(100);
        b.Property(x => x.OccasionDate); // nullable DateOnly — annual occasion

        b.HasOne(x => x.User).WithMany(u => u.Contacts)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);

        b.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> b)
    {
        b.ToTable("Feedbacks");
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).IsRequired().HasMaxLength(200);
        b.Property(x => x.Content).IsRequired().HasMaxLength(2000);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.AdminNote).HasMaxLength(1000);

        b.HasOne(x => x.User).WithMany(u => u.Feedbacks)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class EmailRetryQueueConfiguration : IEntityTypeConfiguration<EmailRetryQueue>
{
    public void Configure(EntityTypeBuilder<EmailRetryQueue> b)
    {
        b.ToTable("EmailRetryQueues");
        b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.LastError).HasMaxLength(2000);

        b.HasIndex(x => new { x.Status, x.NextRetryAt })
            .HasDatabaseName("IX_EmailRetryQueues_Status_NextRetryAt");

        b.HasOne(x => x.GreetingTransaction)
            .WithOne(t => t.RetryQueue)
            .HasForeignKey<EmailRetryQueue>(x => x.GreetingTransactionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SystemLogConfiguration : IEntityTypeConfiguration<SystemLog>
{
    public void Configure(EntityTypeBuilder<SystemLog> b)
    {
        b.ToTable("SystemLogs");
        b.HasKey(x => x.Id);
        // BR-33: Append-only – no UpdatedAt, no soft delete
        b.Property(x => x.Description).IsRequired().HasMaxLength(1000);
        b.Property(x => x.ActorType).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.EventType).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.IpAddress).HasMaxLength(50);
        b.Property(x => x.AdditionalData).HasMaxLength(2000);

        b.HasIndex(x => x.Timestamp).HasDatabaseName("IX_SystemLogs_Timestamp");
        b.HasIndex(x => x.EventType).HasDatabaseName("IX_SystemLogs_EventType");
    }
}

public class WebsiteContentConfiguration : IEntityTypeConfiguration<WebsiteContent>
{
    public void Configure(EntityTypeBuilder<WebsiteContent> b)
    {
        b.ToTable("WebsiteContents");
        b.HasKey(x => x.Id);
        b.Property(x => x.Section).IsRequired().HasMaxLength(50);
        b.Property(x => x.Key).IsRequired().HasMaxLength(100);
        b.Property(x => x.Value);
        b.Property(x => x.ContentType).HasMaxLength(20);

        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
