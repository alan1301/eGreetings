using EGreetings.Application.Interfaces;
using EGreetings.Domain.Exceptions;
using EGreetings.Shared.Constants;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Commands.Cards.AutoSaveDraft;

/// <summary>UC05 – BR-09: Auto-save every 30 seconds via PATCH /drafts/{id}.</summary>
public record AutoSaveDraftCommand(
    Guid DraftId,
    Guid UserId,
    string? PersonalMessage,
    string? CustomJsonContent
) : IRequest<Unit>;

public class AutoSaveDraftCommandValidator : AbstractValidator<AutoSaveDraftCommand>
{
    public AutoSaveDraftCommandValidator()
    {
        RuleFor(x => x.PersonalMessage)
            .MaximumLength(BusinessConstants.MaxPersonalMessageLength)
            .WithMessage($"Personal message must not exceed {BusinessConstants.MaxPersonalMessageLength} characters.");
    }
}

public class AutoSaveDraftCommandHandler : IRequestHandler<AutoSaveDraftCommand, Unit>
{
    private readonly IAppDbContext _db;

    public AutoSaveDraftCommandHandler(IAppDbContext db) => _db = db;

    public async Task<Unit> Handle(AutoSaveDraftCommand request, CancellationToken ct)
    {
        var draft = await _db.Drafts
            .FirstOrDefaultAsync(d => d.Id == request.DraftId && d.UserId == request.UserId && !d.IsDeleted, ct)
            ?? throw new EntityNotFoundException("Draft", request.DraftId);

        draft.PersonalMessage = request.PersonalMessage;
        draft.CustomJsonContent = request.CustomJsonContent;
        draft.LastAutoSavedAt = DateTime.UtcNow;
        draft.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
