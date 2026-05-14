using EGreetings.Application.Interfaces;
using EGreetings.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Commands.Cards.DeleteDraft;

/// <summary>
/// UC17 – Delete a draft owned by the current user.
/// Called automatically by the frontend after a card is sent successfully (UC06)
/// so the sent draft does not linger in the user's draft list.
/// </summary>
public record DeleteDraftCommand(
    Guid DraftId,
    // Set by controller from JWT — NOT required in request body
    Guid UserId = default
) : IRequest;

public class DeleteDraftCommandHandler : IRequestHandler<DeleteDraftCommand>
{
    private readonly IAppDbContext _db;

    public DeleteDraftCommandHandler(IAppDbContext db) => _db = db;

    public async Task Handle(DeleteDraftCommand request, CancellationToken ct)
    {
        var draft = await _db.Drafts
            .FirstOrDefaultAsync(d => d.Id == request.DraftId && d.UserId == request.UserId, ct);

        // Silently succeed if the draft doesn't exist or belongs to another user.
        // This keeps the send flow clean even when the user never explicitly saved a draft.
        if (draft is null) return;

        _db.Drafts.Remove(draft);
        await _db.SaveChangesAsync(ct);
    }
}
