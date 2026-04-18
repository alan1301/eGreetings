using EGreetings.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Identity.Application.Common.Interfaces;

public interface IIdentityDbContext
{
    DbSet<User> Users { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
