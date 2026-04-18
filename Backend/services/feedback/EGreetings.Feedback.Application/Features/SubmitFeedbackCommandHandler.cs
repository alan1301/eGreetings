using MediatR;
using EGreetings.Feedback.Application.Common.Interfaces;
using EGreetings.Feedback.Domain.Entities;
using EGreetings.Feedback.Domain.Enums;

namespace EGreetings.Feedback.Application.Features;

public class SubmitFeedbackCommandHandler : IRequestHandler<SubmitFeedbackCommand, int>
{
    private readonly IFeedbackDbContext _context;

    public SubmitFeedbackCommandHandler(IFeedbackDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(SubmitFeedbackCommand request, CancellationToken cancellationToken)
    {
        // Validate star rating if provided
        if (request.StarRating.HasValue && (request.StarRating < 1 || request.StarRating > 5))
        {
            throw new InvalidOperationException("Star rating must be between 1 and 5");
        }

        var feedback = new Domain.Entities.Feedback
        {
            UserId = request.UserId,
            Subject = request.Subject,
            Content = request.Content,
            ContactEmail = request.ContactEmail,
            StarRating = request.StarRating,
            Status = FeedbackStatus.New,
            IsRead = false
        };

        await _context.Feedbacks.AddAsync(feedback, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return feedback.Id.GetHashCode();
    }
}
