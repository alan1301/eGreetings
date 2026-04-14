using EGreetings.Application.Features.Templates.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Templates.Queries.GetTemplates;

/// <summary>UC03 - Xem danh sách mẫu thiệp | UC23 - View Home Page</summary>
public record GetTemplatesQuery(
    int? CategoryId = null,
    string? SearchKeyword = null,
    bool? IsFree = null,
    int Page = 1,
    int PageSize = 12
) : IRequest<List<TemplateDto>>;
