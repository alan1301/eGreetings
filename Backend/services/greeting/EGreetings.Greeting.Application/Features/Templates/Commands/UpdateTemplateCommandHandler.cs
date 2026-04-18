using EGreetings.Greeting.Application.Common.Interfaces;
using EGreetings.Greeting.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Greeting.Application.Features.Templates.Commands;

public class UpdateTemplateCommandHandler : IRequestHandler<UpdateTemplateCommand, TemplateDto>
{
    private readonly IGreetingDbContext _context;

    public UpdateTemplateCommandHandler(IGreetingDbContext context)
    {
        _context = context;
    }

    public async Task<TemplateDto> Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.GreetingTemplates
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Template with id {request.Id} not found");

        if (!string.IsNullOrWhiteSpace(request.Name))
            template.Name = request.Name;

        if (request.Description != null)
            template.Description = request.Description;

        if (request.ThumbnailUrl != null)
            template.ThumbnailUrl = request.ThumbnailUrl;

        if (!string.IsNullOrWhiteSpace(request.HtmlContent))
            template.HtmlContent = request.HtmlContent;

        if (request.CssStyle != null)
            template.CssStyle = request.CssStyle;

        if (request.IsFree.HasValue)
            template.IsFree = request.IsFree.Value;

        if (request.IsActive.HasValue)
            template.IsActive = request.IsActive.Value;

        template.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == template.CategoryId, cancellationToken);

        return new TemplateDto(
            template.Id,
            template.CategoryId,
            category?.Name ?? "",
            template.Name,
            template.ThumbnailUrl,
            template.IsFree,
            template.IsActive,
            template.UsageCount,
            template.CreatedAt
        );
    }
}
