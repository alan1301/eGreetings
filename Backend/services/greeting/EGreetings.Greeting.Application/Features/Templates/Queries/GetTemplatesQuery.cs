using EGreetings.Greeting.Application.Common.Models;
using EGreetings.Greeting.Application.DTOs;
using MediatR;

namespace EGreetings.Greeting.Application.Features.Templates.Queries;

public record GetTemplatesQuery(
    int? CategoryId,
    string? Keyword,
    bool? IsFree,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<TemplateDto>>;
