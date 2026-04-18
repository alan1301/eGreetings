using MediatR;

namespace EGreetings.Feedback.Application.Features;

public record ReplyFeedbackCommand(
    int FeedbackId,
    int AdminUserId,
    string Reply
) : IRequest<bool>;
