using EGreetings.Application.Interfaces;
using EGreetings.Domain.Entities;
using EGreetings.Domain.Enums;
using EGreetings.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace EGreetings.Application.Commands.Admin.Cards;

// ────────────────────────────────────────────────────────────────────
// UC09 – Admin: Create card (BR-17: Admin only)
// ────────────────────────────────────────────────────────────────────
public record CreateCardCommand(
    Guid CategoryId,
    string Name,
    string? Description,
    string? Tags,
    string? ThumbnailUrl,
    string? FileUrl,
    string? CustomJsonContent,
    bool IsPremium = true,
    bool IsFeatured = false
) : IRequest<Guid>;

public class CreateCardCommandValidator : AbstractValidator<CreateCardCommand>
{
    public CreateCardCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public class CreateCardCommandHandler : IRequestHandler<CreateCardCommand, Guid>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogService _audit;

    public CreateCardCommandHandler(IAppDbContext db, IAuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<Guid> Handle(CreateCardCommand request, CancellationToken ct)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && !c.IsDeleted, ct)
            ?? throw new EntityNotFoundException("Category", request.CategoryId);

        var slug = request.Name.ToLower().Replace(" ", "-") + "-" + Guid.NewGuid().ToString("N")[..6];

        var card = new GreetingCard
        {
            CategoryId = request.CategoryId,
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description,
            Tags = request.Tags,
            ThumbnailUrl = request.ThumbnailUrl,
            FileUrl = request.FileUrl,
            CustomJsonContent = request.CustomJsonContent,
            IsPremium = request.IsPremium,
            IsFeatured = request.IsFeatured,
            Status = CardStatus.Active
        };

        await _db.GreetingCards.AddAsync(card, ct);
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(EventType.AdminAction, $"[UC09] Admin thêm mẫu thiệp: {card.Name}",
            actorType: ActorType.Admin, cancellationToken: ct);

        return card.Id;
    }
}

// ────────────────────────────────────────────────────────────────────
// UC10 – Admin: Archive card (BR-19: soft Inactive)
// ────────────────────────────────────────────────────────────────────
public record ArchiveCardCommand(Guid CardId) : IRequest<Unit>;

public class ArchiveCardCommandHandler : IRequestHandler<ArchiveCardCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogService _audit;

    public ArchiveCardCommandHandler(IAppDbContext db, IAuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<Unit> Handle(ArchiveCardCommand request, CancellationToken ct)
    {
        var card = await _db.GreetingCards
            .FirstOrDefaultAsync(c => c.Id == request.CardId && !c.IsDeleted, ct)
            ?? throw new EntityNotFoundException("GreetingCard", request.CardId);

        // BR-19: Soft Inactive – do not hard delete
        card.Status = CardStatus.Inactive;
        card.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(EventType.AdminAction, $"[UC10] Admin ẩn mẫu thiệp: {card.Name}",
            actorType: ActorType.Admin, cancellationToken: ct);

        return Unit.Value;
    }
}

// ────────────────────────────────────────────────────────────────────
// UC10 – Admin: Hard delete card (only if no transactions – BR-18)
// ────────────────────────────────────────────────────────────────────
public record DeleteCardCommand(Guid CardId) : IRequest<Unit>;

public class DeleteCardCommandHandler : IRequestHandler<DeleteCardCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogService _audit;

    public DeleteCardCommandHandler(IAppDbContext db, IAuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<Unit> Handle(DeleteCardCommand request, CancellationToken ct)
    {
        var card = await _db.GreetingCards
            .FirstOrDefaultAsync(c => c.Id == request.CardId && !c.IsDeleted, ct)
            ?? throw new EntityNotFoundException("GreetingCard", request.CardId);

        // BR-18: Cannot hard delete if has transaction history
        var hasTransactions = await _db.GreetingTransactions
            .AnyAsync(t => t.CardId == request.CardId, ct);

        if (hasTransactions)
            throw new BusinessRuleViolationException("BR-18",
                "Đã có lịch sử giao dịch. Dùng chức năng Ẩn thiệp thay vì xóa.");

        card.IsDeleted = true;
        card.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(EventType.AdminAction, $"[UC10] Admin xóa mẫu thiệp: {card.Name}",
            actorType: ActorType.Admin, cancellationToken: ct);

        return Unit.Value;
    }
}

// ────────────────────────────────────────────────────────────────────
// UC09 – Admin: Update card (BR-17: Admin only)
// ────────────────────────────────────────────────────────────────────
public record UpdateCardCommand(
    Guid CardId,
    Guid CategoryId,
    string Name,
    string? Description,
    string? Tags,
    string? ThumbnailUrl,
    string? FileUrl,
    string? CustomJsonContent,
    bool IsPremium = true,
    bool IsFeatured = false
) : IRequest<Unit>;

public class UpdateCardCommandValidator : AbstractValidator<UpdateCardCommand>
{
    public UpdateCardCommandValidator()
    {
        RuleFor(x => x.CardId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public class UpdateCardCommandHandler : IRequestHandler<UpdateCardCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogService _audit;

    public UpdateCardCommandHandler(IAppDbContext db, IAuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<Unit> Handle(UpdateCardCommand request, CancellationToken ct)
    {
        var card = await _db.GreetingCards
            .FirstOrDefaultAsync(c => c.Id == request.CardId && !c.IsDeleted, ct)
            ?? throw new EntityNotFoundException("GreetingCard", request.CardId);

        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && !c.IsDeleted, ct)
            ?? throw new EntityNotFoundException("Category", request.CategoryId);

        card.CategoryId = request.CategoryId;
        card.Name = request.Name.Trim();
        // Option to regenerate slug or keep old one. We'll keep the old slug to avoid breaking existing links
        card.Description = request.Description;
        card.Tags = request.Tags;
        card.ThumbnailUrl = request.ThumbnailUrl;
        card.FileUrl = request.FileUrl;
        card.CustomJsonContent = request.CustomJsonContent;
        card.IsPremium = request.IsPremium;
        card.IsFeatured = request.IsFeatured;
        card.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(EventType.AdminAction, $"[UC09] Admin cập nhật mẫu thiệp: {card.Name}",
            actorType: ActorType.Admin, cancellationToken: ct);

        return Unit.Value;
    }
}
