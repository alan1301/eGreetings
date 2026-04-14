using EGreetings.Application.Common.Interfaces;
using EGreetings.Domain.Entities;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Admin.Commands.ManageWebContent;

public class UpdateWebContentCommandHandler : IRequestHandler<UpdateWebContentCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;

    private const int MaxVersionsToKeep = 10; // Lưu tối đa 10 phiên bản để rollback

    public UpdateWebContentCommandHandler(IApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<int> Handle(UpdateWebContentCommand request, CancellationToken cancellationToken)
    {
        // Deactivate phiên bản hiện tại
        var currentActive = await _context.WebContents
            .Where(wc => wc.Key == request.Key && wc.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        int? previousVersionId = null;
        int newVersion = 1;

        if (currentActive != null)
        {
            currentActive.IsActive = false;
            currentActive.UpdatedAt = DateTime.UtcNow;
            previousVersionId = currentActive.Id;
            newVersion = currentActive.Version + 1;
        }

        // Tạo phiên bản mới
        var newContent = new WebContent
        {
            Key = request.Key,
            Title = request.Title,
            Content = request.Content,
            ImageUrl = request.ImageUrl,
            IsActive = true,
            Version = newVersion,
            PreviousVersionId = previousVersionId,
            UpdatedByUserId = request.UpdatedByUserId
        };

        _context.WebContents.Add(newContent);
        await _context.SaveChangesAsync(cancellationToken);

        // Dọn dẹp: Giữ tối đa 10 phiên bản cũ
        var oldVersions = await _context.WebContents
            .Where(wc => wc.Key == request.Key && !wc.IsActive)
            .OrderByDescending(wc => wc.Version)
            .ThenBy(wc => wc.Id)
            .Skip(MaxVersionsToKeep)
            .ToListAsync(cancellationToken);

        if (oldVersions.Count != 0)
        {
            foreach (var old in oldVersions)
                _context.WebContents.Remove(old);
            await _context.SaveChangesAsync(cancellationToken);
        }

        await _auditService.LogAsync(AuditEventType.WebContentUpdated, "WebContent",
            newContent.Id.ToString(),
            $"Nội dung '{request.Key}' cập nhật lên v{newVersion}", cancellationToken: cancellationToken);

        return newContent.Id;
    }
}
