using EGreetings.Application.Common.Interfaces;
using EGreetings.Domain.Entities;
using MediatR;

namespace EGreetings.Application.Features.Users.Commands.SaveDraft;

public class SaveDraftCommandHandler : IRequestHandler<SaveDraftCommand, int>
{
    private readonly IUnitOfWork _uow;

    public SaveDraftCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<int> Handle(SaveDraftCommand request, CancellationToken cancellationToken)
    {
        var draft = new Draft
        {
            UserId = request.UserId,
            TemplateId = request.TemplateId,
            Title = request.Title ?? "Bản nháp chưa đặt tên",
            CustomHtml = request.CustomHtml ?? string.Empty,
            RecipientEmail = request.RecipientEmail,
            RecipientName = request.RecipientName,
            SenderMessage = request.SenderMessage
        };

        _uow.Repository<Draft>().Add(draft);
        await _uow.SaveChangesAsync(cancellationToken);

        return draft.Id;
    }
}
