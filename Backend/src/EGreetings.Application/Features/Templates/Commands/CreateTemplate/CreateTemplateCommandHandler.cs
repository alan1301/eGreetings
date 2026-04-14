using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Templates.DTOs;
using EGreetings.Domain.Entities;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Templates.Commands.CreateTemplate;

public class CreateTemplateCommandHandler : IRequestHandler<CreateTemplateCommand, TemplateDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public CreateTemplateCommandHandler(IApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<TemplateDto> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("Danh mục không tồn tại.");

        var template = new GreetingTemplate
        {
            CategoryId = request.CategoryId,
            Name = request.Name,
            Description = request.Description,
            ThumbnailUrl = request.ThumbnailUrl,
            HtmlContent = request.HtmlContent,
            CssStyle = request.CssStyle,
            IsFree = request.IsFree,
            IsActive = true
        };

        _context.GreetingTemplates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(AuditEventType.TemplateCreated, "GreetingTemplate",
            template.Id.ToString(), $"Template '{template.Name}' được tạo.", cancellationToken: cancellationToken);

        return new TemplateDto(template.Id, template.CategoryId, category.Name,
            template.Name, template.Description, template.ThumbnailUrl,
            template.IsFree, template.IsActive, template.UsageCount, template.CreatedAt);
    }
}
