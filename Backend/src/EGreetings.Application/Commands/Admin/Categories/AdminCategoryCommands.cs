using EGreetings.Application.Interfaces;
using EGreetings.Domain.Entities;
using EGreetings.Domain.Enums;
using EGreetings.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Commands.Admin.Categories;

// ────────────────────────────────────────────────────────────────────
// UC27 – Create Category (BR-17)
// ────────────────────────────────────────────────────────────────────
public record CreateCategoryCommand(
    string Name,
    string? Description,
    string? IconUrl,
    string? Color,
    int DisplayOrder = 0
) : IRequest<Guid>;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogService _audit;

    public CreateCategoryCommandHandler(IAppDbContext db, IAuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var slug = request.Name.ToLower().Trim().Replace(" ", "-");
        var slugExists = await _db.Categories.AnyAsync(c => c.Slug == slug && !c.IsDeleted, ct);
        if (slugExists)
            throw new BusinessRuleViolationException("UC27", "A category with this name already exists.");

        var category = new Category
        {
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description,
            IconUrl = request.IconUrl,
            Color = request.Color,
            DisplayOrder = request.DisplayOrder,
            Status = CategoryStatus.Active
        };

        await _db.Categories.AddAsync(category, ct);
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(EventType.AdminAction, $"[UC27] Created category: {category.Name}",
            actorType: ActorType.Admin, cancellationToken: ct);

        return category.Id;
    }
}

// ────────────────────────────────────────────────────────────────────
// UC27 – Hide Category (BR-31: no hard delete if has active cards)
// ────────────────────────────────────────────────────────────────────
public record HideCategoryCommand(Guid CategoryId) : IRequest<Unit>;

public class HideCategoryCommandHandler : IRequestHandler<HideCategoryCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogService _audit;

    public HideCategoryCommandHandler(IAppDbContext db, IAuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<Unit> Handle(HideCategoryCommand request, CancellationToken ct)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && !c.IsDeleted, ct)
            ?? throw new EntityNotFoundException("Category", request.CategoryId);

        if (category.IsSystem)
            throw new BusinessRuleViolationException("BR-31", "System categories cannot be hidden.");

        // BR-31: Cannot hard delete if has active cards – just hide
        category.Status = CategoryStatus.Hidden;
        category.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(EventType.AdminAction, $"[UC27] Hidden category: {category.Name}",
            actorType: ActorType.Admin, cancellationToken: ct);

        return Unit.Value;
    }
}
