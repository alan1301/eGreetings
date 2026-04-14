using EGreetings.Application.Common.Interfaces;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Admin.Commands.ManageCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public DeleteCategoryCommandHandler(IApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken)
            ?? throw new KeyNotFoundException("Danh mục không tồn tại.");

        // BR-31: Không xóa danh mục hệ thống
        if (category.IsSystem)
            throw new InvalidOperationException("Không thể xóa danh mục hệ thống.");

        // BR-31: Không xóa nếu có template đang hoạt động
        var hasActiveTemplates = await _context.GreetingTemplates
            .AnyAsync(t => t.CategoryId == request.CategoryId && t.IsActive, cancellationToken);

        if (hasActiveTemplates)
            throw new InvalidOperationException(
                "Không thể xóa danh mục vì còn mẫu thiệp đang hoạt động. Vui lòng vô hiệu hóa hoặc chuyển mẫu sang danh mục khác trước.");

        // Soft delete
        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(AuditEventType.CategoryDeleted, "Category",
            category.Id.ToString(), $"Danh mục '{category.Name}' bị xóa.", cancellationToken: cancellationToken);

        return true;
    }
}
