using EGreetings.Identity.Application.Common.Interfaces;
using EGreetings.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Identity.Infrastructure.Persistence;

public class IdentityDbContext : DbContext, IIdentityDbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        mb.Entity<User>(b =>
        {
            b.HasKey(u => u.Id);
            b.HasIndex(u => u.Email).IsUnique();
            b.Property(u => u.Email).HasMaxLength(256).IsRequired();
            b.Property(u => u.FullName).HasMaxLength(100).IsRequired();
            b.Property(u => u.Role).HasMaxLength(20).HasDefaultValue("User");
            b.Property(u => u.Status).HasMaxLength(20).HasDefaultValue("Pending");
            b.Property(u => u.FailedLoginAttempts).HasDefaultValue(0);
        });
    }
}
