using MediatR;

namespace EGreetings.Application.Features.Admin.Commands.ManageWebContent;

/// <summary>UC28 - Quản lý nội dung website (Banner, Footer, About) - Admin. Hỗ trợ rollback 10 phiên bản.</summary>
public record UpdateWebContentCommand(
    string Key,
    string Title,
    string Content,
    string? ImageUrl,
    int UpdatedByUserId
) : IRequest<int>;  // Returns new WebContent Id
