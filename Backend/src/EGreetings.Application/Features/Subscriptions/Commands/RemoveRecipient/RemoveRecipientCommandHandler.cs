using EGreetings.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Subscriptions.Commands.RemoveRecipient;

public class RemoveRecipientCommandHandler : IRequestHandler<RemoveRecipientCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public RemoveRecipientCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RemoveRecipientCommand request, CancellationToken cancellationToken)
    {
        // Verify subscription belongs to user
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId && s.UserId == request.UserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Gói Subscribe không tồn tại hoặc không có quyền truy cập.");

        var recipient = await _context.SubscriptionRecipients
            .FirstOrDefaultAsync(r => r.Id == request.RecipientId && r.SubscriptionId == request.SubscriptionId, cancellationToken)
            ?? throw new KeyNotFoundException("Người nhận không tồn tại.");

        recipient.IsActive = false;
        _context.SubscriptionRecipients.Update(recipient);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
