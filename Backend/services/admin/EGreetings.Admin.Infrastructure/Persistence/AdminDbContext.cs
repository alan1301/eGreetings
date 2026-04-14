using EGreetings.Admin.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Admin.Infrastructure.Persistence;

public class AdminDbContext : DbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options) { }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<WebContent> WebContents => Set<WebContent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.EntityName).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<WebContent>(e =>
        {
            e.HasKey(w => w.Id);
            e.HasIndex(w => new { w.Key, w.IsActive });
        });
    }
}
