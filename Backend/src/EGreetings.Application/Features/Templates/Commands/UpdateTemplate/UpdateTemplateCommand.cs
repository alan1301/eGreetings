using EGreetings.Application.Features.Templates.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Templates.Commands.UpdateTemplate;

/// <summary>UC10 - Cập nhật mẫu thiệp (Admin only)</summary>
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
