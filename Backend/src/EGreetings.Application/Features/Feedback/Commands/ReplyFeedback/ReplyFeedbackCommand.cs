using MediatR;

namespace EGreetings.Application.Features.Feedback.Commands.ReplyFeedback;

/// <summary>UC11 - Trả lời phản hồi (Admin)</summary>
public record ReplyFeedbackCommand(
    int FeedbackId,
    int AdminUserId,
    string Reply
) : IRequest<bool>;
