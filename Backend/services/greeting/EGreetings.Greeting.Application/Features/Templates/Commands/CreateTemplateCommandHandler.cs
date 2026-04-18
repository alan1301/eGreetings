using EGreetings.Greeting.Application.Common.Interfaces;
using EGreetings.Greeting.Application.DTOs;
using EGreetings.Greeting.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Greeting.Application.Features.Templates.Commands;

public class CreateTemplateCommandHandler : IRequestHandler<CreateTemplateCommand, TemplateDto>
{
    private readonly IGreetingDbContext _context;

    public CreateTemplateCommandHandler(IGreetingDbContext context)
    {
        _context = context;
    }

    public async Task<TemplateDto> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken)
            ?? throw new KeyNotFoundException($"Category with id {request.CategoryId} not found");

        var template = new GreetingTemplate
        {
            CategoryId = request.CategoryId,
            Name = request.Name,
            Description = request.Description,
            ThumbnailUrl = request.ThumbnailUrl,
            HtmlContent = request.HtmlContent,
            CssStyle = request.CssStyle,
            IsFree = request.IsFree,
            IsActive = true,
            UsageCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.GreetingTemplates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);

        return new TemplateDto(
            template.Id,
            template.CategoryId,
            category.Name,
            template.Name,
            template.ThumbnailUrl,
            template.IsFree,
            template.IsActive,
            template.UsageCount,
            template.CreatedAt
        );
    }
}
