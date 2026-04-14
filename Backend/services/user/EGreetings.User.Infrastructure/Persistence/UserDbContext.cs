using EGreetings.User.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.User.Infrastructure.Persistence;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Draft> Drafts => Set<Draft>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserProfile>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.IdentityUserId).IsUnique();
            e.Property(u => u.Email).IsRequired().HasMaxLength(200);
            e.Property(u => u.FullName).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Contact>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(100);
            e.Property(c => c.Email).IsRequired().HasMaxLength(200);
        });
    }
}
