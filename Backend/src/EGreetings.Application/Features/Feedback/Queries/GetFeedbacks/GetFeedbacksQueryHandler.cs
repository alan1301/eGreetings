using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Admin.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Feedback.Queries.GetFeedbacks;

public class GetFeedbacksQueryHandler : IRequestHandler<GetFeedbacksQuery, IReadOnlyList<FeedbackDto>>
{
    private readonly IRepository<EGreetings.Domain.Entities.Feedback> _feedbackRepo;

    public GetFeedbacksQueryHandler(IRepository<EGreetings.Domain.Entities.Feedback> feedbackRepo)
    {
        _feedbackRepo = feedbackRepo;
    }

    public async Task<IReadOnlyList<FeedbackDto>> Handle(GetFeedbacksQuery request, CancellationToken cancellationToken)
    {
        var feedbacks = await _feedbackRepo.Query()
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FeedbackDto(
                f.Id, f.UserId, f.Subject, f.Content,
                f.ContactEmail, f.StarRating,
                f.IsRead, f.AdminReply, f.RepliedAt, f.CreatedAt))
            .ToListAsync(cancellationToken);

        return feedbacks;
    }
}
