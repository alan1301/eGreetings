using MediatR;
using Microsoft.EntityFrameworkCore;
using EGreetings.Subscription.Application.Common.Interfaces;
using EGreetings.Subscription.Application.DTOs;

namespace EGreetings.Subscription.Application.Features.Subscriptions.Queries;

public class GetMySubscriptionQueryHandler : IRequestHandler<GetMySubscriptionQuery, SubscriptionDetailDto?>
{
    private readonly ISubscriptionDbContext _context;

    public GetMySubscriptionQueryHandler(ISubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<SubscriptionDetailDto?> Handle(GetMySubscriptionQuery request, CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .Include(s => s.Recipients)
            .FirstOrDefaultAsync(s => s.UserId == request.UserId, cancellationToken);

        if (subscription == null)
        {
            return null;
        }

        var recipients = subscription.Recipients
            .Select(r => new SubscriptionRecipientDto(
                r.Id,
                r.Email,
                r.Name,
                r.Birthday,
                r.Occasion,
                r.IsActive
            ))
            .ToList();

        return new SubscriptionDetailDto(
            subscription.Id,
            subscription.UserId,
            subscription.Plan?.Name ?? "Unknown",
            subscription.Status.ToString(),
            subscription.Plan?.PricePerYear ?? 0,
            subscription.StartDate,
            subscription.ExpiredAt,
            subscription.MaxRecipients,
            subscription.Recipients.Count(r => r.IsActive),
            subscription.CreatedAt,
            recipients
        );
    }
}
