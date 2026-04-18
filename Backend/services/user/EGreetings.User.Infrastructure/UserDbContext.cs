using Microsoft.EntityFrameworkCore;
using EGreetings.User.Application.Common.Interfaces;
using EGreetings.User.Domain.Entities;

namespace EGreetings.User.Infrastructure;

public class UserDbContext : DbContext, IUserDbContext
{
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Draft> Drafts { get; set; }

    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // UserProfile
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
        });

        // Contact
        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.Group).HasMaxLength(100);
        });

        // Draft
        modelBuilder.Entity<Draft>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.RecipientEmail).HasMaxLength(255);
            entity.Property(e => e.RecipientName).HasMaxLength(255);
        });
    }
}
