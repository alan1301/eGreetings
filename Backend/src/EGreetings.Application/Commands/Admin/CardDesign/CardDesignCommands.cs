using EGreetings.Application.Interfaces;
using EGreetings.Domain.Entities;
using EGreetings.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Commands.Admin.CardDesign;

// ═══════════════════════════════════════════════════════════════════════
// BACKGROUNDS
// ═══════════════════════════════════════════════════════════════════════

public record CreateCardBackgroundCommand(
    string Id, string Label, string BgStyle,
    string Categories, bool IsPremium = false, int SortOrder = 0
) : IRequest<string>;

public class CreateCardBackgroundHandler : IRequestHandler<CreateCardBackgroundCommand, string>
{
    private readonly IAppDbContext _db;
    public CreateCardBackgroundHandler(IAppDbContext db) => _db = db;

    public async Task<string> Handle(CreateCardBackgroundCommand r, CancellationToken ct)
    {
        if (await _db.CardBackgrounds.AnyAsync(b => b.Id == r.Id, ct))
            throw new InvalidOperationException($"Background id '{r.Id}' already exists.");

        var bg = new CardBackground
        {
            Id = r.Id.Trim().ToLower(),
            Label = r.Label.Trim(),
            BgStyle = r.BgStyle.Trim(),
            Categories = r.Categories,
            IsPremium = r.IsPremium,
            SortOrder = r.SortOrder
        };
        _db.CardBackgrounds.Add(bg);
        await _db.SaveChangesAsync(ct);
        return bg.Id;
    }
}

public record UpdateCardBackgroundCommand(
    string Id, string Label, string BgStyle,
    string Categories, bool IsPremium, bool IsActive, int SortOrder
) : IRequest;

public class UpdateCardBackgroundHandler : IRequestHandler<UpdateCardBackgroundCommand>
{
    private readonly IAppDbContext _db;
    public UpdateCardBackgroundHandler(IAppDbContext db) => _db = db;

    public async Task Handle(UpdateCardBackgroundCommand r, CancellationToken ct)
    {
        var bg = await _db.CardBackgrounds.FindAsync([r.Id], ct)
            ?? throw new EntityNotFoundException("CardBackground", r.Id);

        bg.Label = r.Label.Trim();
        bg.BgStyle = r.BgStyle.Trim();
        bg.Categories = r.Categories;
        bg.IsPremium = r.IsPremium;
        bg.IsActive = r.IsActive;
        bg.SortOrder = r.SortOrder;
        bg.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }
}

public record ToggleCardBackgroundCommand(string Id) : IRequest<bool>;

public class ToggleCardBackgroundHandler : IRequestHandler<ToggleCardBackgroundCommand, bool>
{
    private readonly IAppDbContext _db;
    public ToggleCardBackgroundHandler(IAppDbContext db) => _db = db;

    public async Task<bool> Handle(ToggleCardBackgroundCommand r, CancellationToken ct)
    {
        var bg = await _db.CardBackgrounds.FindAsync([r.Id], ct)
            ?? throw new EntityNotFoundException("CardBackground", r.Id);
        bg.IsActive = !bg.IsActive;
        bg.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return bg.IsActive;
    }
}

public record DeleteCardBackgroundCommand(string Id) : IRequest;

public class DeleteCardBackgroundHandler : IRequestHandler<DeleteCardBackgroundCommand>
{
    private readonly IAppDbContext _db;
    public DeleteCardBackgroundHandler(IAppDbContext db) => _db = db;

    public async Task Handle(DeleteCardBackgroundCommand r, CancellationToken ct)
    {
        var bg = await _db.CardBackgrounds.FindAsync([r.Id], ct)
            ?? throw new EntityNotFoundException("CardBackground", r.Id);
        _db.CardBackgrounds.Remove(bg);
        await _db.SaveChangesAsync(ct);
    }
}

// ═══════════════════════════════════════════════════════════════════════
// DECORATIONS
// ═══════════════════════════════════════════════════════════════════════

public record CreateCardDecorationCommand(
    string Id, string Label, string Preview,
    string Categories, string Elements, int SortOrder = 0
) : IRequest<string>;

public class CreateCardDecorationHandler : IRequestHandler<CreateCardDecorationCommand, string>
{
    private readonly IAppDbContext _db;
    public CreateCardDecorationHandler(IAppDbContext db) => _db = db;

    public async Task<string> Handle(CreateCardDecorationCommand r, CancellationToken ct)
    {
        if (await _db.CardDecorations.AnyAsync(d => d.Id == r.Id, ct))
            throw new InvalidOperationException($"Decoration id '{r.Id}' already exists.");

        var decor = new CardDecoration
        {
            Id = r.Id.Trim().ToLower(),
            Label = r.Label.Trim(),
            Preview = r.Preview.Trim(),
            Categories = r.Categories,
            Elements = r.Elements,
            SortOrder = r.SortOrder
        };
        _db.CardDecorations.Add(decor);
        await _db.SaveChangesAsync(ct);
        return decor.Id;
    }
}

public record UpdateCardDecorationCommand(
    string Id, string Label, string Preview,
    string Categories, string Elements, bool IsActive, int SortOrder
) : IRequest;

public class UpdateCardDecorationHandler : IRequestHandler<UpdateCardDecorationCommand>
{
    private readonly IAppDbContext _db;
    public UpdateCardDecorationHandler(IAppDbContext db) => _db = db;

    public async Task Handle(UpdateCardDecorationCommand r, CancellationToken ct)
    {
        var decor = await _db.CardDecorations.FindAsync([r.Id], ct)
            ?? throw new EntityNotFoundException("CardDecoration", r.Id);

        decor.Label = r.Label.Trim();
        decor.Preview = r.Preview.Trim();
        decor.Categories = r.Categories;
        decor.Elements = r.Elements;
        decor.IsActive = r.IsActive;
        decor.SortOrder = r.SortOrder;
        decor.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }
}

public record ToggleCardDecorationCommand(string Id) : IRequest<bool>;

public class ToggleCardDecorationHandler : IRequestHandler<ToggleCardDecorationCommand, bool>
{
    private readonly IAppDbContext _db;
    public ToggleCardDecorationHandler(IAppDbContext db) => _db = db;

    public async Task<bool> Handle(ToggleCardDecorationCommand r, CancellationToken ct)
    {
        var decor = await _db.CardDecorations.FindAsync([r.Id], ct)
            ?? throw new EntityNotFoundException("CardDecoration", r.Id);
        decor.IsActive = !decor.IsActive;
        decor.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return decor.IsActive;
    }
}

public record DeleteCardDecorationCommand(string Id) : IRequest;

public class DeleteCardDecorationHandler : IRequestHandler<DeleteCardDecorationCommand>
{
    private readonly IAppDbContext _db;
    public DeleteCardDecorationHandler(IAppDbContext db) => _db = db;

    public async Task Handle(DeleteCardDecorationCommand r, CancellationToken ct)
    {
        var decor = await _db.CardDecorations.FindAsync([r.Id], ct)
            ?? throw new EntityNotFoundException("CardDecoration", r.Id);
        _db.CardDecorations.Remove(decor);
        await _db.SaveChangesAsync(ct);
    }
}
