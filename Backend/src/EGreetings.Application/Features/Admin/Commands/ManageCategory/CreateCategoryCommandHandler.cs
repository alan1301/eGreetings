using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Categories.DTOs;
using EGreetings.Domain.Entities;
using EGreetings.Domain.Enums;
using MediatR;

namespace EGreetings.Application.Features.Admin.Commands.ManageCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public CreateCategoryCommandHandler(IApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            IconUrl = request.IconUrl,
            IsActive = true,
            DisplayOrder = 0
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(AuditEventType.CategoryCreated, "Category", category.Id.ToString(),
            $"Tạo danh mục '{category.Name}'.", cancellationToken: cancellationToken);

        return new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.IconUrl,
            category.IsActive,
            0,
            category.CreatedAt
        );
    }
}
