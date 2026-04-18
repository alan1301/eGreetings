using MediatR;
using EGreetings.Feedback.Application.DTOs;

namespace EGreetings.Feedback.Application.Features;

public record GetFeedbacksQuery(
    string? Status,
    int Page,
    int PageSize
) : IRequest<PagedResult<FeedbackDto>>;
