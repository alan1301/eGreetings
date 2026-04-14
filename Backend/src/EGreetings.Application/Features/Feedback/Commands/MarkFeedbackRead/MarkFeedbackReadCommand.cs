using MediatR;

namespace EGreetings.Application.Features.Feedback.Commands.MarkFeedbackRead;

/// <summary>UC11 - Đánh dấu phản hồi đã đọc</summary>
public record MarkFeedbackReadCommand(int FeedbackId) : IRequest<bool>;
