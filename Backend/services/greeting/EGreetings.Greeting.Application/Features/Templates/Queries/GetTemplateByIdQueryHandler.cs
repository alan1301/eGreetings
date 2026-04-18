using EGreetings.Greeting.Application.Common.Interfaces;
using EGreetings.Greeting.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Greeting.Application.Features.Templates.Queries;

public class GetTemplateByIdQueryHandler : IRequestHandler<GetTemplateByIdQuery, TemplateDetailDto>
{
    private readonly IGreetingDbContext _context;

    public GetTemplateByIdQueryHandler(IGreetingDbContext context)
    {
        _context = context;
    }

    public async Task<TemplateDetailDto> Handle(GetTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var template = await _context.GreetingTemplates
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Template with id {request.Id} not found");

        if (!template.IsActive)
        {
            throw new KeyNotFoundException($"Template with id {request.Id} not found");
        }

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == template.CategoryId, cancellationToken);

        return new TemplateDetailDto(
            template.Id,
            template.CategoryId,
            category?.Name ?? "",
            template.Name,
            template.ThumbnailUrl,
            template.IsFree,
            template.IsActive,
            template.UsageCount,
            template.CreatedAt,
            template.Description,
            template.HtmlContent,
            template.CssStyle
        );
    }
}
