using EGreetings.Application.Interfaces;
using EGreetings.Domain.Enums;
using EGreetings.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Commands.Cards.CancelScheduledGreeting;

/// <summary>
/// UC06 – Cancel a scheduled greeting before it is sent.
/// Only the original sender can cancel; only Scheduled status is cancellable.
/// </summary>
public record CancelScheduledGreetingCommand(
    Guid TransactionId,
    // Set by controller from JWT — NOT required in request body
    Guid UserId = default
) : IRequest;

public class CancelScheduledGreetingCommandHandler : IRequestHandler<CancelScheduledGreetingCommand>
{
    private readonly IAppDbContext _db;

    public CancelScheduledGreetingCommandHandler(IAppDbContext db) => _db = db;

    public async Task Handle(CancelScheduledGreetingCommand request, CancellationToken ct)
    {
        var transaction = await _db.GreetingTransactions
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId && t.SenderId == request.UserId, ct)
            ?? throw new EntityNotFoundException("GreetingTransaction", request.TransactionId);

        if (transaction.Status != TransactionStatus.Scheduled)
            throw new BusinessRuleViolationException("CANCEL_ONLY_SCHEDULED",
                "Only scheduled greetings can be cancelled.");

        transaction.Status = TransactionStatus.Cancelled;
        await _db.SaveChangesAsync(ct);
    }
}
