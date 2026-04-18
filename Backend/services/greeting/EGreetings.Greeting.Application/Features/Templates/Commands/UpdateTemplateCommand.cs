using EGreetings.Greeting.Application.DTOs;
using MediatR;

namespace EGreetings.Greeting.Application.Features.Templates.Commands;

public record UpdateTemplateCommand(
    int Id,
    string? Name,
    string? Description,
    string? ThumbnailUrl,
    string? HtmlContent,
    string? CssStyle,
    bool? IsFree,
    bool? IsActive
) : IRequest<TemplateDto>;
