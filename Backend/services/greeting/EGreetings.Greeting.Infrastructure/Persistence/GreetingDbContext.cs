using EGreetings.Greeting.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Greeting.Infrastructure.Persistence;

public class GreetingDbContext : DbContext
{
    public GreetingDbContext(DbContextOptions<GreetingDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<GreetingTemplate> Templates => Set<GreetingTemplate>();
    public DbSet<GreetingCard> Greetings => Set<GreetingCard>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GreetingTemplate>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Templates)
            .HasForeignKey(t => t.CategoryId);

        modelBuilder.Entity<GreetingCard>()
            .HasOne(g => g.Template)
            .WithMany()
            .HasForeignKey(g => g.TemplateId);

        modelBuilder.Entity<GreetingCard>()
            .HasIndex(g => g.UniqueToken)
            .IsUnique();
    }
}
