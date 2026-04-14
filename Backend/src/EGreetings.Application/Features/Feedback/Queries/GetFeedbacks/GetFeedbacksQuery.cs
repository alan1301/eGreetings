using EGreetings.Application.Features.Admin.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Feedback.Queries.GetFeedbacks;

/// <summary>UC13 - Admin xem danh sách phản hồi</summary>
public record GetFeedbacksQuery : IRequest<IReadOnlyList<FeedbackDto>>;
