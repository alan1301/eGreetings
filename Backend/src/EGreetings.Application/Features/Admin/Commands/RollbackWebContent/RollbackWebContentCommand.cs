using MediatR;

namespace EGreetings.Application.Features.Admin.Commands.RollbackWebContent;

/// <summary>UC28 - Rollback nội dung website về phiên bản cũ</summary>
public record RollbackWebContentCommand(
    string Key,
    int VersionId,
    int AdminUserId
) : IRequest<bool>;
