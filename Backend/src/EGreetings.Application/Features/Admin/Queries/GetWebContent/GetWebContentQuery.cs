using EGreetings.Application.Features.Admin.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Admin.Queries.GetWebContent;

/// <summary>UC28 - Lấy nội dung website theo key</summary>
public record GetWebContentQuery(string Key) : IRequest<WebContentDto?>;
