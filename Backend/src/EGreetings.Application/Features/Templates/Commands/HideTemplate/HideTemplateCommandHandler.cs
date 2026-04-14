using EGreetings.Application.Common.Interfaces;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Templates.Commands.HideTemplate;

public class HideTemplateCommandHandler : IRequestHandler<HideTemplateCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public HideTemplateCommandHandler(IApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<bool> Handle(HideTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.GreetingTemplates
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Mẫu thiệp không tồn tại.");

        template.IsActive = !request.Hide;
        _context.GreetingTemplates.Update(template);
        await _context.SaveChangesAsync(cancellationToken);

        var action = request.Hide ? "Ẩn" : "Hiện";
        await _auditService.LogAsync(AuditEventType.TemplateUpdated, "GreetingTemplate", template.Id.ToString(),
            $"{action} mẫu thiệp '{template.Name}'.", cancellationToken: cancellationToken);

        return true;
    }
}
