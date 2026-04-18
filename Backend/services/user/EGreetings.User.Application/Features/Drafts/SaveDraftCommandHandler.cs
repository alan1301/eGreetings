using MediatR;
using EGreetings.User.Application.Common.Interfaces;
using EGreetings.User.Domain.Entities;

namespace EGreetings.User.Application.Features.Drafts;

public class SaveDraftCommandHandler : IRequestHandler<SaveDraftCommand, int>
{
    private readonly IUserDbContext _context;

    public SaveDraftCommandHandler(IUserDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(SaveDraftCommand request, CancellationToken cancellationToken)
    {
        var draft = new Draft
        {
            UserId = request.UserId,
            TemplateId = request.TemplateId,
            Title = request.Title,
            CustomHtml = request.CustomHtml,
            RecipientEmail = request.RecipientEmail,
            RecipientName = request.RecipientName,
            SenderMessage = request.SenderMessage
        };

        await _context.Drafts.AddAsync(draft, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return draft.Id.GetHashCode();
    }
}
