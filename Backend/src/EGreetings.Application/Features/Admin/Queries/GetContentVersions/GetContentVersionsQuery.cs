using EGreetings.Application.Features.Admin.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Admin.Queries.GetContentVersions;

/// <summary>UC28 - Xem lịch sử phiên bản nội dung website (để rollback)</summary>
public record GetContentVersionsQuery(string Key) : IRequest<IReadOnlyList<WebContentVersionDto>>;
