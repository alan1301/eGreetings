using MediatR;
using Microsoft.EntityFrameworkCore;
using EGreetings.Feedback.Application.Common.Interfaces;
using EGreetings.Feedback.Domain.Enums;

namespace EGreetings.Feedback.Application.Features;

public class ReplyFeedbackCommandHandler : IRequestHandler<ReplyFeedbackCommand, bool>
{
    private readonly IFeedbackDbContext _context;

    public ReplyFeedbackCommandHandler(IFeedbackDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ReplyFeedbackCommand request, CancellationToken cancellationToken)
    {
        var feedback = await _context.Feedbacks
            .FirstOrDefaultAsync(f => f.Id == request.FeedbackId, cancellationToken);

        if (feedback == null)
        {
            throw new InvalidOperationException("Feedback not found");
        }

        feedback.Reply = request.Reply;
        feedback.RepliedAt = DateTime.UtcNow;
        feedback.RepliedByAdminId = request.AdminUserId;
        feedback.Status = FeedbackStatus.Replied;
        feedback.IsRead = true;

        _context.Feedbacks.Update(feedback);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
