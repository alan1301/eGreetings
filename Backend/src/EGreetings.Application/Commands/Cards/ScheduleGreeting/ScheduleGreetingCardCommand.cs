using EGreetings.Application.Interfaces;
using EGreetings.Domain.Entities;
using EGreetings.Domain.Enums;
using EGreetings.Domain.Exceptions;
using EGreetings.Shared.Constants;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Commands.Cards.ScheduleGreeting;

/// <summary>UC06 Alternative Flow A1 – BR-11: Scheduled at >= UtcNow + 5 min.</summary>
public record ScheduleGreetingCardCommand(
    Guid SenderId,
    string SenderEmail,
    Guid CardId,
    string RecipientEmail,
    string Subject,
    string? PersonalMessage,
    DateTime ScheduledSendAt
) : IRequest<Guid>;

public class ScheduleGreetingCardCommandValidator : AbstractValidator<ScheduleGreetingCardCommand>
{
    public ScheduleGreetingCardCommandValidator()
    {
        RuleFor(x => x.RecipientEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PersonalMessage)
            .MaximumLength(BusinessConstants.MaxPersonalMessageLength);
        // BR-11
        RuleFor(x => x.ScheduledSendAt)
            .GreaterThan(DateTime.UtcNow.AddMinutes(BusinessConstants.ScheduledSendMinutesAhead))
            .WithMessage($"Thời gian hẹn giờ phải ít nhất {BusinessConstants.ScheduledSendMinutesAhead} phút sau thời điểm hiện tại");
    }
}

public class ScheduleGreetingCardCommandHandler : IRequestHandler<ScheduleGreetingCardCommand, Guid>
{
    private readonly IAppDbContext _db;

    public ScheduleGreetingCardCommandHandler(IAppDbContext db) => _db = db;

    public async Task<Guid> Handle(ScheduleGreetingCardCommand request, CancellationToken ct)
    {
        var card = await _db.GreetingCards
            .FirstOrDefaultAsync(c => c.Id == request.CardId && c.Status == CardStatus.Active && !c.IsDeleted, ct)
            ?? throw new EntityNotFoundException("GreetingCard", request.CardId);

        // BR-12: Always log transaction
        var transaction = new GreetingTransaction
        {
            SenderId = request.SenderId,
            CardId = request.CardId,
            RecipientEmail = request.RecipientEmail.ToLower(),
            Subject = request.Subject,
            PersonalMessage = request.PersonalMessage,
            ScheduledSendAt = request.ScheduledSendAt.ToUniversalTime(),
            Status = TransactionStatus.Scheduled,
        };

        await _db.GreetingTransactions.AddAsync(transaction, ct);
        await _db.SaveChangesAsync(ct);

        return transaction.Id;
    }
}
