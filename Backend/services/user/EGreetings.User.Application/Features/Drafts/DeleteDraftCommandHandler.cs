using MediatR;
using Microsoft.EntityFrameworkCore;
using EGreetings.User.Application.Common.Interfaces;

namespace EGreetings.User.Application.Features.Drafts;

public class DeleteDraftCommandHandler : IRequestHandler<DeleteDraftCommand, bool>
{
    private readonly IUserDbContext _context;

    public DeleteDraftCommandHandler(IUserDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteDraftCommand request, CancellationToken cancellationToken)
    {
        var draft = await _context.Drafts
            .FirstOrDefaultAsync(d => d.Id == request.DraftId && d.UserId == request.UserId, cancellationToken);

        if (draft == null)
        {
            throw new InvalidOperationException("Draft not found");
        }

        _context.Drafts.Remove(draft);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
