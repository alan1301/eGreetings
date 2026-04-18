using EGreetings.Admin.Application.Common.Interfaces;
using EGreetings.Admin.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Admin.Infrastructure.Persistence;

public class AdminDbContext : DbContext, IAdminDbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options)
    {
    }

    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<WebContent> WebContents { get; set; } = null!;
    public DbSet<WebContentVersion> WebContentVersions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EventType)
                .HasConversion<int>();

            entity.Property(e => e.EntityName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Description)
                .IsRequired();

            entity.Property(e => e.UserEmail)
                .HasMaxLength(255);

            entity.Property(e => e.IpAddress)
                .HasMaxLength(45);

            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.EntityName);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.OccurredAt).IsDescending();
        });

        modelBuilder.Entity<WebContent>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Key)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Content)
                .IsRequired();

            entity.Property(e => e.ImageUrl)
                .HasMaxLength(1000);

            entity.HasIndex(e => e.Key).IsUnique();

            entity.HasMany(e => e.Versions)
                .WithOne(v => v.WebContent)
                .HasForeignKey(v => v.WebContentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WebContentVersion>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Content)
                .IsRequired();

            entity.Property(e => e.ImageUrl)
                .HasMaxLength(1000);

            entity.HasIndex(e => new { e.WebContentId, e.Version });
            entity.HasIndex(e => e.ArchivedAt);
        });
    }
}
