using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Categories.DTOs;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Admin.Commands.ManageCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public UpdateCategoryCommandHandler(IApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Danh mục không tồn tại.");

        if (!string.IsNullOrWhiteSpace(request.Name))
            category.Name = request.Name;

        if (!string.IsNullOrWhiteSpace(request.Description))
            category.Description = request.Description;

        if (!string.IsNullOrWhiteSpace(request.IconUrl))
            category.IconUrl = request.IconUrl;

        if (request.IsActive.HasValue)
            category.IsActive = request.IsActive.Value;

        _context.Categories.Update(category);
        await _context.SaveChangesAsync(cancellationToken);

        // Count active templates in a separate query to avoid lazy-loading issues
        var templateCount = await _context.GreetingTemplates
            .CountAsync(t => t.CategoryId == category.Id && t.IsActive, cancellationToken);

        await _auditService.LogAsync(AuditEventType.CategoryUpdated, "Category", category.Id.ToString(),
            $"Cập nhật danh mục '{category.Name}'.", cancellationToken: cancellationToken);

        return new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.IconUrl,
            category.IsActive,
            templateCount,
            category.CreatedAt
        );
    }
}
