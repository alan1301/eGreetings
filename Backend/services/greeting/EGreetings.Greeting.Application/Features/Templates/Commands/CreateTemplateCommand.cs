using EGreetings.Greeting.Application.DTOs;
using MediatR;

namespace EGreetings.Greeting.Application.Features.Templates.Commands;

public record CreateTemplateCommand(
    int CategoryId,
    string Name,
    string? Description,
    string? ThumbnailUrl,
    string HtmlContent,
    string? CssStyle,
    bool IsFree
) : IRequest<TemplateDto>;
