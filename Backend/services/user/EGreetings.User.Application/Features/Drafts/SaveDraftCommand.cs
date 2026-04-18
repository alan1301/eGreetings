using MediatR;

namespace EGreetings.User.Application.Features.Drafts;

public record SaveDraftCommand(
    int UserId,
    int TemplateId,
    string? Title,
    string? CustomHtml,
    string? RecipientEmail,
    string? RecipientName,
    string? SenderMessage
) : IRequest<int>;
