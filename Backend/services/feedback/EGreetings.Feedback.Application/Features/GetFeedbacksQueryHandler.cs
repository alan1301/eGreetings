using MediatR;
using Microsoft.EntityFrameworkCore;
using EGreetings.Feedback.Application.Common.Interfaces;
using EGreetings.Feedback.Application.DTOs;
using EGreetings.Feedback.Domain.Enums;

namespace EGreetings.Feedback.Application.Features;

public class GetFeedbacksQueryHandler : IRequestHandler<GetFeedbacksQuery, PagedResult<FeedbackDto>>
{
    private readonly IFeedbackDbContext _context;

    public GetFeedbacksQueryHandler(IFeedbackDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<FeedbackDto>> Handle(GetFeedbacksQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Feedbacks.AsQueryable();

        // Filter by status
        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<FeedbackStatus>(request.Status, out var statusEnum))
            {
                query = query.Where(f => f.Status == statusEnum);
            }
        }

        var total = await query.CountAsync(cancellationToken);

        var feedbacks = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(f => new FeedbackDto(
                f.Id,
                f.UserId,
                f.Subject,
                f.Content,
                f.StarRating,
                f.Status.ToString(),
                f.Reply,
                f.RepliedAt,
                f.IsRead,
                f.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<FeedbackDto>(feedbacks, total, request.Page, request.PageSize);
    }
}
