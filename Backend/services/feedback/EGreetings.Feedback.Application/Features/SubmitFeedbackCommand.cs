using MediatR;

namespace EGreetings.Feedback.Application.Features;

public record SubmitFeedbackCommand(
    int? UserId,
    string Subject,
    string Content,
    string? ContactEmail,
    int? StarRating
) : IRequest<int>;
