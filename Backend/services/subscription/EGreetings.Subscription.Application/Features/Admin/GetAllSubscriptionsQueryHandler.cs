using MediatR;
using Microsoft.EntityFrameworkCore;
using EGreetings.Subscription.Application.Common.Interfaces;
using EGreetings.Subscription.Application.DTOs;
using EGreetings.Subscription.Domain.Enums;

namespace EGreetings.Subscription.Application.Features.Admin;

public class GetAllSubscriptionsQueryHandler : IRequestHandler<GetAllSubscriptionsQuery, PagedResult<SubscriptionDto>>
{
    private readonly ISubscriptionDbContext _context;

    public GetAllSubscriptionsQueryHandler(ISubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<SubscriptionDto>> Handle(GetAllSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Subscriptions
            .Include(s => s.Plan)
            .Include(s => s.Recipients)
            .AsQueryable();

        // Filter by status
        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<SubscriptionStatus>(request.Status, out var statusEnum))
            {
                query = query.Where(s => s.Status == statusEnum);
            }
        }

        // Filter by userId
        if (request.UserId.HasValue)
        {
            query = query.Where(s => s.UserId == request.UserId);
        }

        var total = await query.CountAsync(cancellationToken);

        var subscriptions = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new SubscriptionDto(
                s.Id,
                s.UserId,
                s.Plan!.Name,
                s.Status.ToString(),
                s.Plan!.PricePerYear,
                s.StartDate,
                s.ExpiredAt,
                s.MaxRecipients,
                s.Recipients.Count(r => r.IsActive),
                s.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<SubscriptionDto>(subscriptions, total, request.Page, request.PageSize);
    }
}
