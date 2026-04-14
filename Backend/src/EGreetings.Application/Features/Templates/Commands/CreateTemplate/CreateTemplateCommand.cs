using EGreetings.Application.Features.Templates.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Templates.Commands.CreateTemplate;

/// <summary>UC12 - Quản lý template (Admin - Thêm mới)</summary>
public record CreateTemplateCommand(
    int CategoryId,
    string Name,
    string Description,
    string ThumbnailUrl,
    string HtmlContent,
    string CssStyle,
    bool IsFree
) : IRequest<TemplateDto>;
