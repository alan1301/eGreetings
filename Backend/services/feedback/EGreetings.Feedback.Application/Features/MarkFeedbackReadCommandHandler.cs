using MediatR;
using Microsoft.EntityFrameworkCore;
using EGreetings.Feedback.Application.Common.Interfaces;
using EGreetings.Feedback.Domain.Enums;

namespace EGreetings.Feedback.Application.Features;

public class MarkFeedbackReadCommandHandler : IRequestHandler<MarkFeedbackReadCommand, bool>
{
    private readonly IFeedbackDbContext _context;

    public MarkFeedbackReadCommandHandler(IFeedbackDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(MarkFeedbackReadCommand request, CancellationToken cancellationToken)
    {
        var feedback = await _context.Feedbacks
            .FirstOrDefaultAsync(f => f.Id == request.FeedbackId, cancellationToken);

        if (feedback == null)
        {
            throw new InvalidOperationException("Feedback not found");
        }

        if (!feedback.IsRead)
        {
            feedback.IsRead = true;
            feedback.Status = FeedbackStatus.Read;

            _context.Feedbacks.Update(feedback);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return true;
    }
}
