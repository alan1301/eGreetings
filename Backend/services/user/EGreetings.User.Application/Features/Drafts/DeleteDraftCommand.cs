using MediatR;

namespace EGreetings.User.Application.Features.Drafts;

public record DeleteDraftCommand(
    int UserId,
    int DraftId
) : IRequest<bool>;
