using EGreetings.Application.Interfaces;
using EGreetings.Domain.Enums;
using EGreetings.Shared.Common;
using EGreetings.Shared.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Queries.Cards;

// ─────────────── DTOs ───────────────
public record CardDto(
    Guid Id, string Name, string Slug, string? ThumbnailUrl, string? CustomJsonContent, string? Description,
    bool IsFeatured, bool IsPremium, string CategoryName, string CategorySlug, string Status);

public record CardDetailDto(
    Guid Id, string Name, string Slug, string? ThumbnailUrl, string? FileUrl,
    string? Description, string? Tags, string? CustomJsonContent, bool IsFeatured, bool IsPremium,
    string CategoryName, string CategorySlug);

public record CategoryDto(
    Guid Id, string Name, string Slug, string? Description, string? IconUrl,
    string? Color, int CardCount);

// ─────────────── Get Cards (UC03, public) ───────────────
public record GetCardsQuery(
    string? CategorySlug = null,
    string? Search = null,
    string? Sort = "newest",    // newest | popular | az
    bool? FeaturedOnly = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<CardDto>>;

public class GetCardsQueryHandler : IRequestHandler<GetCardsQuery, PagedResult<CardDto>>
{
    private readonly IAppDbContext _db;

    public GetCardsQueryHandler(IAppDbContext db) => _db = db;

    public async Task<PagedResult<CardDto>> Handle(GetCardsQuery request, CancellationToken ct)
    {
        var pageSize = Math.Min(request.PageSize, BusinessConstants.MaxPageSize);

        var query = _db.GreetingCards
            .Include(c => c.Category)
            .Where(c => c.Status == CardStatus.Active && !c.IsDeleted
                     && c.Category.Status == CategoryStatus.Active && !c.Category.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.CategorySlug))
            query = query.Where(c => c.Category.Slug == request.CategorySlug);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(c => c.Name.Contains(request.Search) ||
                                     (c.Tags != null && c.Tags.Contains(request.Search)));

        if (request.FeaturedOnly == true)
            query = query.Where(c => c.IsFeatured);

        query = request.Sort switch
        {
            "az" => query.OrderBy(c => c.Name),
            "popular" => query.OrderByDescending(c => c.Transactions.Count),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CardDto(
                c.Id, c.Name, c.Slug, c.ThumbnailUrl, c.CustomJsonContent, c.Description,
                c.IsFeatured, c.IsPremium, c.Category.Name, c.Category.Slug, c.Status.ToString()))
            .ToListAsync(ct);

        return new PagedResult<CardDto>
        {
            Items = items,
            Meta = new PaginationMeta { Page = request.Page, PageSize = pageSize, Total = total }
        };
    }
}

// ──────────── Get Card Detail (UC04, UC24) ────────────
public record GetCardByIdQuery(Guid CardId) : IRequest<CardDetailDto?>;

public class GetCardByIdQueryHandler : IRequestHandler<GetCardByIdQuery, CardDetailDto?>
{
    private readonly IAppDbContext _db;

    public GetCardByIdQueryHandler(IAppDbContext db) => _db = db;

    public async Task<CardDetailDto?> Handle(GetCardByIdQuery request, CancellationToken ct)
        => await _db.GreetingCards
            .Include(c => c.Category)
            .Where(c => c.Id == request.CardId && c.Status == CardStatus.Active && !c.IsDeleted)
            .Select(c => new CardDetailDto(
                c.Id, c.Name, c.Slug, c.ThumbnailUrl, c.FileUrl,
                c.Description, c.Tags, c.CustomJsonContent, c.IsFeatured, c.IsPremium, c.Category.Name, c.Category.Slug))
            .FirstOrDefaultAsync(ct);
}

// ──────────── Get Categories (public) ────────────
public record GetCategoriesQuery : IRequest<List<CategoryDto>>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly IAppDbContext _db;

    public GetCategoriesQueryHandler(IAppDbContext db) => _db = db;

    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken ct)
        => await _db.Categories
            .Where(c => c.Status == CategoryStatus.Active && !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CategoryDto(
                c.Id, c.Name, c.Slug, c.Description, c.IconUrl, c.Color,
                c.Cards.Count(card => card.Status == CardStatus.Active && !card.IsDeleted)))
            .ToListAsync(ct);
}
