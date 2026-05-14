using EGreetings.Application.Interfaces;
using EGreetings.Domain.Enums;
using EGreetings.Shared.Common;
using EGreetings.Shared.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Queries.Admin;

// ──── Admin: Feedback List (UC11) ────
public record FeedbackDto(
    Guid Id, string UserEmail, string UserFullName, string Title, string Content,
    int? StarRating, string Status, DateTime CreatedAt);

public record GetFeedbacksQuery(
    string? Status = null,
    int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<FeedbackDto>>;

public class GetFeedbacksQueryHandler : IRequestHandler<GetFeedbacksQuery, PagedResult<FeedbackDto>>
{
    private readonly IAppDbContext _db;

    public GetFeedbacksQueryHandler(IAppDbContext db) => _db = db;

    public async Task<PagedResult<FeedbackDto>> Handle(GetFeedbacksQuery request, CancellationToken ct)
    {
        var pageSize = Math.Min(request.PageSize, BusinessConstants.MaxPageSize);
        var query = _db.Feedbacks
            .Include(f => f.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<FeedbackStatus>(request.Status, out var status))
            query = query.Where(f => f.Status == status);

        query = query.OrderByDescending(f => f.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Page - 1) * pageSize).Take(pageSize)
            .Select(f => new FeedbackDto(f.Id, f.User.Email, f.User.FullName, f.Title, f.Content,
                f.StarRating, f.Status.ToString(), f.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<FeedbackDto>
        {
            Items = items,
            Meta = new PaginationMeta { Page = request.Page, PageSize = pageSize, Total = total }
        };
    }
}
