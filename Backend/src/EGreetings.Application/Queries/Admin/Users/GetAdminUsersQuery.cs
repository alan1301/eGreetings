using EGreetings.Application.Interfaces;
using EGreetings.Domain.Enums;
using EGreetings.Shared.Common;
using EGreetings.Shared.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Queries.Admin;

// ──── Admin: Users List (UC21) ────
public record AdminUserDto(
    Guid Id, string FullName, string Email, string Role, string Status,
    string? SubscriptionStatus, string? SubscriptionPlan, DateTime? SubscriptionExpiry, Guid? SubscriptionId,
    int TotalCardsSent, DateTime CreatedAt);

public record GetAdminUsersQuery(
    string? Search = null,
    string? Role = null,              // "User" | "Admin" | null = all
    string? AccountStatus = null,     // "Active" | "Locked" | null = all
    string? SubscriptionPlan = null,  // "Free" | "Monthly" | "Annual" | null = all
    int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<AdminUserDto>>;

public class GetAdminUsersQueryHandler : IRequestHandler<GetAdminUsersQuery, PagedResult<AdminUserDto>>
{
    private readonly IAppDbContext _db;

    public GetAdminUsersQueryHandler(IAppDbContext db) => _db = db;

    public async Task<PagedResult<AdminUserDto>> Handle(GetAdminUsersQuery request, CancellationToken ct)
    {
        var pageSize = Math.Min(request.PageSize, BusinessConstants.MaxPageSize);

        var projected = _db.Users
            .Where(u => !u.IsDeleted)
            .Select(u => new
            {
                u.Id, u.FullName, u.Email, u.Role, u.Status, u.CreatedAt,
                LatestSub = u.Subscriptions.OrderByDescending(s => s.CreatedAt)
                    .Select(s => new { s.Id, s.Status, s.Plan, s.ExpiryDate })
                    .FirstOrDefault(),
                TotalSent = u.SentTransactions.Count()
            });

        // Filter: search
        if (!string.IsNullOrEmpty(request.Search))
            projected = projected.Where(x => x.FullName.Contains(request.Search) || x.Email.Contains(request.Search));

        // Filter: role
        if (!string.IsNullOrEmpty(request.Role) && Enum.TryParse<UserRole>(request.Role, out var roleFilter))
            projected = projected.Where(x => x.Role == roleFilter);

        // Filter: account status
        if (!string.IsNullOrEmpty(request.AccountStatus) && Enum.TryParse<UserStatus>(request.AccountStatus, out var statusFilter))
            projected = projected.Where(x => x.Status == statusFilter);

        // Filter: subscription plan ("Free" = no active sub, "Monthly"/"Annual" = active with that plan)
        if (!string.IsNullOrEmpty(request.SubscriptionPlan))
        {
            if (request.SubscriptionPlan == "Free")
                projected = projected.Where(x =>
                    x.LatestSub == null ||
                    x.LatestSub.Status != SubscriptionStatus.Active);
            else if (Enum.TryParse<SubscriptionPlan>(request.SubscriptionPlan, out var planFilter))
                projected = projected.Where(x =>
                    x.LatestSub != null &&
                    x.LatestSub.Status == SubscriptionStatus.Active &&
                    x.LatestSub.Plan == planFilter);
        }

        projected = projected.OrderByDescending(x => x.CreatedAt);

        var total = await projected.CountAsync(ct);
        var items = await projected
            .Skip((request.Page - 1) * pageSize).Take(pageSize)
            .Select(x => new AdminUserDto(
                x.Id, x.FullName, x.Email, x.Role.ToString(), x.Status.ToString(),
                x.LatestSub != null ? x.LatestSub.Status.ToString() : null,
                x.LatestSub != null ? x.LatestSub.Plan.ToString() : null,
                x.LatestSub != null ? x.LatestSub.ExpiryDate : null,
                x.LatestSub != null ? x.LatestSub.Id : null,
                x.TotalSent, x.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<AdminUserDto>
        {
            Items = items,
            Meta = new PaginationMeta { Page = request.Page, PageSize = pageSize, Total = total }
        };
    }
}
