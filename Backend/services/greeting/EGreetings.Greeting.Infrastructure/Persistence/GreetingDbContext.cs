using EGreetings.Greeting.Application.Common.Interfaces;
using EGreetings.Greeting.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Greeting.Infrastructure.Persistence;

public class GreetingDbContext : DbContext, IGreetingDbContext
{
    public GreetingDbContext(DbContextOptions<GreetingDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<GreetingTemplate> GreetingTemplates { get; set; } = null!;
    public DbSet<Domain.Entities.Greeting> Greetings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IconUrl).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.DisplayOrder);
        });

        // Configure GreetingTemplate
        modelBuilder.Entity<GreetingTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CategoryId).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);
            entity.Property(e => e.HtmlContent).IsRequired();
            entity.Property(e => e.CssStyle).HasMaxLength(5000);
            entity.Property(e => e.IsFree).HasDefaultValue(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UsageCount).HasDefaultValue(0);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsFree);
        });

        // Configure Greeting
        modelBuilder.Entity<Domain.Entities.Greeting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId);
            entity.Property(e => e.TemplateId).IsRequired();
            entity.Property(e => e.RecipientEmail).IsRequired().HasMaxLength(256);
            entity.Property(e => e.RecipientName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SenderMessage).HasMaxLength(5000);
            entity.Property(e => e.CustomHtml).HasColumnType("nvarchar(max)");
            entity.Property(e => e.ViewToken).IsRequired().HasMaxLength(32).IsUnicode(false);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.ScheduledAt);
            entity.Property(e => e.SentAt);
            entity.Property(e => e.RetryCount).HasDefaultValue(0);

            entity.HasOne(e => e.Template)
                .WithMany()
                .HasForeignKey(e => e.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.ViewToken).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ScheduledAt);
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
