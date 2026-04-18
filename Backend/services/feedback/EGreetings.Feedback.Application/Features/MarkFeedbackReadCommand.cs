using MediatR;

namespace EGreetings.Feedback.Application.Features;

public record MarkFeedbackReadCommand(int FeedbackId) : IRequest<bool>;
