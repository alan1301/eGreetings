using MediatR;

namespace EGreetings.Application.Features.Users.Commands.SaveDraft;

/// <summary>UC17 - Lưu bản nháp thiệp</summary>
public record SaveDraftCommand(
    int UserId,
    int TemplateId,
    string? Title,
    string? CustomHtml,
    string? RecipientEmail,
    string? RecipientName,
    string? SenderMessage
) : IRequest<int>;
