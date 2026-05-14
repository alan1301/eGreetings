using EGreetings.Application.Interfaces;
using EGreetings.Domain.Entities;
using EGreetings.Domain.Exceptions;
using EGreetings.Shared.Constants;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Commands.Cards.CreateDraft;

/// <summary>UC05 – Create a new draft. Max 50 per user.</summary>
public record CreateDraftCommand(
    Guid UserId,
    Guid CardId,
    string? PersonalMessage,
    string? CustomJsonContent
) : IRequest<Guid>;

public class CreateDraftCommandValidator : AbstractValidator<CreateDraftCommand>
{
    public CreateDraftCommandValidator()
    {
        RuleFor(x => x.CardId).NotEmpty();
        // BR-08: PersonalMessage max 500 chars
        RuleFor(x => x.PersonalMessage)
            .MaximumLength(BusinessConstants.MaxPersonalMessageLength)
            .WithMessage($"Personal message must not exceed {BusinessConstants.MaxPersonalMessageLength} characters.");
    }
}

public class CreateDraftCommandHandler : IRequestHandler<CreateDraftCommand, Guid>
{
    private readonly IAppDbContext _db;

    public CreateDraftCommandHandler(IAppDbContext db) => _db = db;

    public async Task<Guid> Handle(CreateDraftCommand request, CancellationToken ct)
    {
        // Max 50 drafts per user
        var draftCount = await _db.Drafts.CountAsync(d => d.UserId == request.UserId && !d.IsDeleted, ct);
        if (draftCount >= BusinessConstants.MaxDraftsPerUser)
            throw new BusinessRuleViolationException("DRAFT", $"Maximum {BusinessConstants.MaxDraftsPerUser} drafts reached. Please delete some to add more.");

        var card = await _db.GreetingCards.FirstOrDefaultAsync(c => c.Id == request.CardId && !c.IsDeleted, ct)
            ?? throw new EntityNotFoundException("GreetingCard", request.CardId);

        if (card.IsPremium)
        {
            var hasActiveSubscription = await _db.Subscriptions
                .AnyAsync(s => s.UserId == request.UserId && s.Status == EGreetings.Domain.Enums.SubscriptionStatus.Active, ct);

            if (!hasActiveSubscription)
                throw new BusinessRuleViolationException("PREMIUM_TEMPLATE",
                    "This card template is for Premium accounts only.");
        }

        var draft = new Draft
        {
            UserId = request.UserId,
            CardId = request.CardId,
            PersonalMessage = request.PersonalMessage,
            CustomJsonContent = request.CustomJsonContent,
            LastAutoSavedAt = DateTime.UtcNow
        };

        await _db.Drafts.AddAsync(draft, ct);
        await _db.SaveChangesAsync(ct);

        return draft.Id;
    }
}
