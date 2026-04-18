using MediatR;
using Microsoft.EntityFrameworkCore;
using EGreetings.Subscription.Application.Common.Interfaces;
using EGreetings.Subscription.Application.DTOs;

namespace EGreetings.Subscription.Application.Features.Subscriptions.Queries;

public class GetRecipientsQueryHandler : IRequestHandler<GetRecipientsQuery, List<SubscriptionRecipientDto>>
{
    private readonly ISubscriptionDbContext _context;

    public GetRecipientsQueryHandler(ISubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<List<SubscriptionRecipientDto>> Handle(GetRecipientsQuery request, CancellationToken cancellationToken)
    {
        // Verify subscription ownership
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId && s.UserId == request.UserId, cancellationToken);

        if (subscription == null)
        {
            throw new InvalidOperationException("Subscription not found or access denied");
        }

        var recipients = await _context.SubscriptionRecipients
            .Where(r => r.SubscriptionId == request.SubscriptionId)
            .Select(r => new SubscriptionRecipientDto(
                r.Id,
                r.Email,
                r.Name,
                r.Birthday,
                r.Occasion,
                r.IsActive
            ))
            .ToListAsync(cancellationToken);

        return recipients;
    }
}
