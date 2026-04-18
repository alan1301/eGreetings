using MediatR;
using Microsoft.EntityFrameworkCore;
using EGreetings.Subscription.Application.Common.Interfaces;
using EGreetings.Subscription.Domain.Enums;

namespace EGreetings.Subscription.Application.Features.Admin;

public class DisableSubscriptionCommandHandler : IRequestHandler<DisableSubscriptionCommand, bool>
{
    private readonly ISubscriptionDbContext _context;

    public DisableSubscriptionCommandHandler(ISubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DisableSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId, cancellationToken);

        if (subscription == null)
        {
            throw new InvalidOperationException("Subscription not found");
        }

        subscription.IsDisabledByAdmin = true;
        subscription.Status = SubscriptionStatus.Disabled;
        subscription.DisabledReason = request.Reason;

        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
