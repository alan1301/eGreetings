using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Templates.DTOs;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Templates.Commands.UpdateTemplate;

public class UpdateTemplateCommandHandler : IRequestHandler<UpdateTemplateCommand, TemplateDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public UpdateTemplateCommandHandler(IApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<TemplateDto> Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.GreetingTemplates
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Mẫu thiệp không tồn tại.");

        if (!string.IsNullOrWhiteSpace(request.Name))
            template.Name = request.Name;

        if (!string.IsNullOrWhiteSpace(request.Description))
            template.Description = request.Description;

        if (!string.IsNullOrWhiteSpace(request.ThumbnailUrl))
            template.ThumbnailUrl = request.ThumbnailUrl;

        if (!string.IsNullOrWhiteSpace(request.HtmlContent))
            template.HtmlContent = request.HtmlContent;

        if (!string.IsNullOrWhiteSpace(request.CssStyle))
            template.CssStyle = request.CssStyle;

        if (request.IsFree.HasValue)
            template.IsFree = request.IsFree.Value;

        if (request.IsActive.HasValue)
            template.IsActive = request.IsActive.Value;

        _context.GreetingTemplates.Update(template);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(AuditEventType.TemplateUpdated, "GreetingTemplate", template.Id.ToString(),
            $"Cập nhật mẫu thiệp '{template.Name}'.", cancellationToken: cancellationToken);

        return new TemplateDto(
            template.Id,
            template.CategoryId,
            template.Category.Name,
            template.Name,
            template.Description,
            template.ThumbnailUrl,
            template.IsFree,
            template.IsActive,
            template.UsageCount,
            template.CreatedAt
        );
    }
}
