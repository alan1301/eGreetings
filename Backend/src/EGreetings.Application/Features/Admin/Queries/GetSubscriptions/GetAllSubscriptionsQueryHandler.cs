using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Subscriptions.DTOs;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Admin.Queries.GetSubscriptions;

public class GetAllSubscriptionsQueryHandler : IRequestHandler<GetAllSubscriptionsQuery, PagedResult<SubscriptionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllSubscriptionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<SubscriptionDto>> Handle(GetAllSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Subscriptions
            .Include(s => s.Recipients)
            .AsQueryable();

        // Filter by status if provided
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (Enum.TryParse<SubscriptionStatus>(request.Status, true, out var status))
                query = query.Where(s => s.Status == status);
        }

        // Filter by user if provided
        if (request.UserId.HasValue)
            query = query.Where(s => s.UserId == request.UserId.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new SubscriptionDto(
                s.Id,
                s.UserId,
                s.Status.ToString(),
                s.Price,
                s.StartDate,
                s.ExpiredAt,
                s.MaxRecipients,
                s.Recipients.Count(r => r.IsActive),
                s.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<SubscriptionDto>(items, total, request.Page, request.PageSize);
    }
}
