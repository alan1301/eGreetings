using EGreetings.Application.Interfaces;
using EGreetings.Domain.Entities;
using EGreetings.Domain.Enums;
using EGreetings.Domain.Events;
using EGreetings.Domain.Exceptions;
using EGreetings.Shared.Constants;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Commands.Cards.SendGreeting;

/// <summary>
/// UC06 – Send greeting now.
/// BR-10: max 50/day non-subscribe. BR-12: always log transaction.
/// BR-28: ReplyTo = sender's email.
/// </summary>
public record SendGreetingCardCommand(
    Guid CardId,
    string RecipientEmail,
    string Subject,
    string? PersonalMessage,
    // Set by controller from JWT — NOT required in request body
    Guid SenderId = default,
    string? SenderEmail = null
) : IRequest<Guid>;

public class SendGreetingCardCommandValidator : AbstractValidator<SendGreetingCardCommand>
{
    public SendGreetingCardCommandValidator()
    {
        RuleFor(x => x.RecipientEmail).NotEmpty().EmailAddress().WithMessage("Invalid recipient email address.");
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PersonalMessage)
            .MaximumLength(BusinessConstants.MaxPersonalMessageLength)
            .WithMessage($"Personal message must not exceed {BusinessConstants.MaxPersonalMessageLength} characters.");
    }
}

public class SendGreetingCardCommandHandler : IRequestHandler<SendGreetingCardCommand, Guid>
{
    private readonly IAppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly IAuditLogService _audit;

    public SendGreetingCardCommandHandler(IAppDbContext db, IEmailService emailService, IAuditLogService audit)
    {
        _db = db;
        _emailService = emailService;
        _audit = audit;
    }

    public async Task<Guid> Handle(SendGreetingCardCommand request, CancellationToken ct)
    {
        // Validate sender exists
        var senderExists = await _db.Users
            .AnyAsync(u => u.Id == request.SenderId && !u.IsDeleted, ct);
        
        if (!senderExists)
            throw new UnauthorizedException("Invalid session. Please log in again.");

        // BR-10: Check daily send limit for non-subscriber
        var hasActiveSubscription = await _db.Subscriptions
            .AnyAsync(s => s.UserId == request.SenderId && s.Status == SubscriptionStatus.Active, ct);

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
                    $"Daily send limit reached ({BusinessConstants.MaxCardsPerDayNonSubscribe} cards/day). Upgrade to a subscription to send unlimited cards.");
        }

        var card = await _db.GreetingCards
            .FirstOrDefaultAsync(c => c.Id == request.CardId && c.Status == CardStatus.Active && !c.IsDeleted, ct)
            ?? throw new EntityNotFoundException("GreetingCard", request.CardId);

        if (card.IsPremium && !hasActiveSubscription)
            throw new BusinessRuleViolationException("PREMIUM_TEMPLATE",
                "This card template is for Premium accounts only.");

        // BR-12: Create transaction record FIRST
        var transaction = new GreetingTransaction
        {
            SenderId = request.SenderId,
            CardId = request.CardId,
            RecipientEmail = request.RecipientEmail.ToLower(),
            Subject = request.Subject,
            PersonalMessage = request.PersonalMessage,
            Status = TransactionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _db.GreetingTransactions.AddAsync(transaction, ct);
        await _db.SaveChangesAsync(ct);

        // Send email (BR-28: ReplyTo = sender's email)
        try
        {
            await _emailService.SendAsync(new EmailMessage
            {
                To = request.RecipientEmail,
                Subject = request.Subject,
                HtmlBody = $"""
                    <div style="font-family: sans-serif;">
                        <h2>You've received a digital greeting card!</h2>
                        <p><strong>Card:</strong> {card.Name}</p>
                        {(string.IsNullOrEmpty(request.PersonalMessage) ? "" : $"<p><strong>Message:</strong> {request.PersonalMessage}</p>")}
                        <img src="{card.ThumbnailUrl}" alt="{card.Name}" style="max-width:600px;" />
                    </div>
                """,
                ReplyTo = request.SenderEmail    // BR-28
            }, ct);

            transaction.Status = TransactionStatus.Sent;
            transaction.SentAt = DateTime.UtcNow;
            transaction.RaiseDomainEvent(new GreetingSentEvent(transaction.Id, transaction.SenderId, transaction.RecipientEmail));
        }
        catch (Exception ex)
        {
            transaction.Status = TransactionStatus.Failed;
            transaction.RetryCount = 0;

            // Create retry queue (BR-32)
            var retry = new EmailRetryQueue
            {
                GreetingTransactionId = transaction.Id,
                NextRetryAt = DateTime.UtcNow.AddMinutes(BusinessConstants.EmailRetryIntervalMinutes),
                LastError = ex.Message,
                Status = RetryStatus.PendingRetry
            };
            await _db.EmailRetryQueues.AddAsync(retry, ct);
        }

        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(EventType.SendCard,
            $"[UC06] Card sent: {card.Name} → {request.RecipientEmail}",
            transaction.Status == TransactionStatus.Sent ? LogStatus.Success : LogStatus.Failed,
            request.SenderId, ActorType.User, cancellationToken: ct);

        return transaction.Id;
    }
}
