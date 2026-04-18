using EGreetings.Greeting.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Greeting.Application.Common.Interfaces;

public interface IGreetingDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<GreetingTemplate> GreetingTemplates { get; }
    DbSet<Domain.Entities.Greeting> Greetings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
