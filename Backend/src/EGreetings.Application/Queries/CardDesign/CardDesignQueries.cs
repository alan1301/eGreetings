using EGreetings.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Queries.CardDesign;

// ─── DTOs ────────────────────────────────────────────────────────────
public record CardBackgroundDto(
    string Id, string Label, string BgStyle,
    string Categories, bool IsPremium, bool IsActive, int SortOrder);

public record CardDecorationDto(
    string Id, string Label, string Preview,
    string Categories, string Elements, bool IsActive, int SortOrder);

// ─── Public: Get active backgrounds ──────────────────────────────────
public record GetCardBackgroundsQuery(bool AdminMode = false) : IRequest<List<CardBackgroundDto>>;

public class GetCardBackgroundsQueryHandler : IRequestHandler<GetCardBackgroundsQuery, List<CardBackgroundDto>>
{
    private readonly IAppDbContext _db;
    public GetCardBackgroundsQueryHandler(IAppDbContext db) => _db = db;

    public async Task<List<CardBackgroundDto>> Handle(GetCardBackgroundsQuery request, CancellationToken ct)
    {
        var query = _db.CardBackgrounds.AsQueryable();
        if (!request.AdminMode)
            query = query.Where(b => b.IsActive);

        return await query
            .OrderBy(b => b.SortOrder).ThenBy(b => b.Label)
            .Select(b => new CardBackgroundDto(
                b.Id, b.Label, b.BgStyle, b.Categories, b.IsPremium, b.IsActive, b.SortOrder))
            .ToListAsync(ct);
    }
}

// ─── Public: Get active decorations ──────────────────────────────────
public record GetCardDecorationsQuery(bool AdminMode = false) : IRequest<List<CardDecorationDto>>;

public class GetCardDecorationsQueryHandler : IRequestHandler<GetCardDecorationsQuery, List<CardDecorationDto>>
{
    private readonly IAppDbContext _db;
    public GetCardDecorationsQueryHandler(IAppDbContext db) => _db = db;

    public async Task<List<CardDecorationDto>> Handle(GetCardDecorationsQuery request, CancellationToken ct)
    {
        var query = _db.CardDecorations.AsQueryable();
        if (!request.AdminMode)
            query = query.Where(d => d.IsActive);

        return await query
            .OrderBy(d => d.SortOrder).ThenBy(d => d.Label)
            .Select(d => new CardDecorationDto(
                d.Id, d.Label, d.Preview, d.Categories, d.Elements, d.IsActive, d.SortOrder))
            .ToListAsync(ct);
    }
}
