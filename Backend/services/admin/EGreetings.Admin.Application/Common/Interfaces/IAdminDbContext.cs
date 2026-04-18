using EGreetings.Admin.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Admin.Application.Common.Interfaces;

public interface IAdminDbContext
{
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<WebContent> WebContents { get; }
    DbSet<WebContentVersion> WebContentVersions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
