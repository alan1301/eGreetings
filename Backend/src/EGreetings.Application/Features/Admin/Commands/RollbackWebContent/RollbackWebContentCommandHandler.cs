using EGreetings.Application.Common.Interfaces;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Admin.Commands.RollbackWebContent;

public class RollbackWebContentCommandHandler : IRequestHandler<RollbackWebContentCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public RollbackWebContentCommandHandler(IApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<bool> Handle(RollbackWebContentCommand request, CancellationToken cancellationToken)
    {
        // Find the version to rollback to
        var versionToRestore = await _context.WebContents
            .FirstOrDefaultAsync(c => c.Id == request.VersionId && c.Key == request.Key, cancellationToken)
            ?? throw new KeyNotFoundException("Phiên bản không tồn tại.");

        // Find the current active content
        var currentContent = await _context.WebContents
            .Where(c => c.Key == request.Key && c.IsActive)
            .OrderByDescending(c => c.Version)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentContent != null)
        {
            currentContent.IsActive = false;
            _context.WebContents.Update(currentContent);
        }

        // Activate the old version
        versionToRestore.IsActive = true;
        _context.WebContents.Update(versionToRestore);

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(AuditEventType.WebContentUpdated, "WebContent", versionToRestore.Id.ToString(),
            $"Rollback nội dung '{request.Key}' về phiên bản {versionToRestore.Version}.",
            cancellationToken: cancellationToken);

        return true;
    }
}
