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
    Guid CardId,
    string RecipientEmail,
    string Subject,
    string? PersonalMessage,
    DateTime ScheduledSendAt,
    // Set by controller from JWT — NOT required in request body
    Guid SenderId = default,
    string? SenderEmail = null
) : IRequest<Guid>;

public class ScheduleGreetingCardCommandValidator : AbstractValidator<ScheduleGreetingCardCommand>
{
    public ScheduleGreetingCardCommandValidator()
    {
        RuleFor(x => x.RecipientEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PersonalMessage)
            .MaximumLength(BusinessConstants.MaxPersonalMessageLength);
        // BR-11: evaluate at request time (lambda), not at validator instantiation time
        RuleFor(x => x.ScheduledSendAt)
            .GreaterThan(_ => DateTime.UtcNow.AddMinutes(BusinessConstants.ScheduledSendMinutesAhead))
            .WithMessage($"Thời gian hẹn giờ phải ít nhất {BusinessConstants.ScheduledSendMinutesAhead} phút sau thời điểm hiện tại");
    }
}

public class ScheduleGreetingCardCommandHandler : IRequestHandler<ScheduleGreetingCardCommand, Guid>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogService _audit;

    public ScheduleGreetingCardCommandHandler(IAppDbContext db, IAuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<Guid> Handle(ScheduleGreetingCardCommand request, CancellationToken ct)
    {
        // Validate sender exists
        var senderExists = await _db.Users
            .AnyAsync(u => u.Id == request.SenderId && !u.IsDeleted, ct);

        if (!senderExists)
            throw new UnauthorizedException("Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.");

        var hasActiveSubscription = await _db.Subscriptions
            .AnyAsync(s => s.UserId == request.SenderId && s.Status == SubscriptionStatus.Active, ct);

        // BR-10: Check daily send limit for non-subscriber (scheduled cards count toward the daily quota)
        if (!hasActiveSubscription)
        {
            var today = DateTime.UtcNow.Date;
            var sentToday = await _db.GreetingTransactions.CountAsync(
                t => t.SenderId == request.SenderId
                     && t.SubscriptionId == null
                     && t.CreatedAt >= today
                     && t.CreatedAt < today.AddDays(1), ct);

            if (sentToday >= BusinessConstants.MaxCardsPerDayNonSubscribe)
                throw new BusinessRuleViolationException("BR-10",
                    $"Đã đạt giới hạn {BusinessConstants.MaxCardsPerDayNonSubscribe} thiệp/ngày. Nâng cấp Subscribe để gửi không giới hạn.");
        }

        var card = await _db.GreetingCards
            .FirstOrDefaultAsync(c => c.Id == request.CardId && c.Status == CardStatus.Active && !c.IsDeleted, ct)
            ?? throw new EntityNotFoundException("GreetingCard", request.CardId);

        if (card.IsPremium && !hasActiveSubscription)
            throw new BusinessRuleViolationException("PREMIUM_TEMPLATE",
                "Mẫu thiệp này chỉ dành cho tài khoản Premium.");

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
            CreatedAt = DateTime.UtcNow
        };

        await _db.GreetingTransactions.AddAsync(transaction, ct);
        await _db.SaveChangesAsync(ct);

        // BR-33: Audit log for scheduled send
        await _audit.LogAsync(EventType.SendCard,
            $"Hẹn giờ gửi thiệp: {card.Name} → {request.RecipientEmail} lúc {transaction.ScheduledSendAt:yyyy-MM-dd HH:mm} UTC",
            LogStatus.Success,
            request.SenderId, ActorType.User, cancellationToken: ct);

        return transaction.Id;
    }
}
