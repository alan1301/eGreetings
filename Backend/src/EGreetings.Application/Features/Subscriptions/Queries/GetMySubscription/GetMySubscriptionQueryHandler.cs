using EGreetings.Application.Features.Subscriptions.DTOs;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using EGreetings.Application.Common.Interfaces;

namespace EGreetings.Application.Features.Subscriptions.Queries.GetMySubscription;

public class GetMySubscriptionQueryHandler : IRequestHandler<GetMySubscriptionQuery, SubscriptionDetailDto?>
{
    private readonly IApplicationDbContext _context;

    public GetMySubscriptionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SubscriptionDetailDto?> Handle(GetMySubscriptionQuery request, CancellationToken cancellationToken)
    {
        // Get most recent active subscription for the user
        var subscription = await _context.Subscriptions
            .Include(s => s.Recipients.Where(r => r.IsActive))
            .Where(s => s.UserId == request.UserId && s.Status == SubscriptionStatus.Active)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription == null)
            return null;

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
            subscription.Status.ToString(),
            subscription.Price,
            subscription.StartDate,
            subscription.ExpiredAt,
            subscription.MaxRecipients,
            subscription.Recipients.Count(r => r.IsActive),
            recipients,
            subscription.CreatedAt
        );
    }
}
