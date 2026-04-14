using EGreetings.Application.Features.Admin.DTOs;
using EGreetings.Application.Features.Users.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Admin.Queries.GetAuditLogs;

/// <summary>UC30 - Xem audit logs (Admin) - BR-33</summary>
public record GetAuditLogsQuery(
    string? EntityName,
    int? UserId,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 50
) : IRequest<PagedResult<AuditLogDto>>;
