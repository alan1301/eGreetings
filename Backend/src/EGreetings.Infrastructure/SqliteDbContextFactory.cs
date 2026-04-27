using EGreetings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EGreetings.Infrastructure;

/// <summary>
/// Design-time factory for SQLite (macOS/Linux dev).
/// Used by 'dotnet ef migrations add' for SQLite provider.
/// </summary>
public class SqliteDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>();
        opts.UseSqlite("Data Source=egreetings_dev.db");
        return new AppDbContext(opts.Options);
    }
}
