using EGreetings.User.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.User.Application.Common.Interfaces;

public interface IUserDbContext
{
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<Contact> Contacts { get; }
    DbSet<Draft> Drafts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
