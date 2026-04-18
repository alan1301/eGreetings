using MediatR;
using Microsoft.EntityFrameworkCore;
using EGreetings.Subscription.Application.Common.Interfaces;

namespace EGreetings.Subscription.Application.Features.Subscriptions.Commands;

public class RemoveRecipientCommandHandler : IRequestHandler<RemoveRecipientCommand, bool>
{
    private readonly ISubscriptionDbContext _context;

    public RemoveRecipientCommandHandler(ISubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RemoveRecipientCommand request, CancellationToken cancellationToken)
    {
        // Verify subscription ownership
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId && s.UserId == request.UserId, cancellationToken);

        if (subscription == null)
        {
            throw new InvalidOperationException("Subscription not found or access denied");
        }

        var recipient = await _context.SubscriptionRecipients
            .FirstOrDefaultAsync(r => r.Id == request.RecipientId && r.SubscriptionId == request.SubscriptionId, cancellationToken);

        if (recipient == null)
        {
            throw new InvalidOperationException("Recipient not found");
        }

        _context.SubscriptionRecipients.Remove(recipient);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
