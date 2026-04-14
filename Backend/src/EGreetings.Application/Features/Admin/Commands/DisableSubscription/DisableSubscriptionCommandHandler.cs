using EGreetings.Application.Common.Interfaces;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Admin.Commands.DisableSubscription;

public class DisableSubscriptionCommandHandler : IRequestHandler<DisableSubscriptionCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public DisableSubscriptionCommandHandler(IApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<bool> Handle(DisableSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId, cancellationToken)
            ?? throw new KeyNotFoundException("Gói Subscribe không tồn tại.");

        subscription.Status = SubscriptionStatus.Disabled;
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        var reason = !string.IsNullOrWhiteSpace(request.Reason)
            ? $" Lý do: {request.Reason}"
            : "";

        await _auditService.LogAsync(AuditEventType.SubscriptionDisabled, "Subscription", subscription.Id.ToString(),
            $"Admin vô hiệu hóa gói Subscribe.{reason}",
            cancellationToken: cancellationToken);

        return true;
    }
}
