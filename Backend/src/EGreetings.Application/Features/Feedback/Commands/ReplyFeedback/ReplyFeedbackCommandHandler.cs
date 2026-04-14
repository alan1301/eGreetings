using EGreetings.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Feedback.Commands.ReplyFeedback;

public class ReplyFeedbackCommandHandler : IRequestHandler<ReplyFeedbackCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ReplyFeedbackCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ReplyFeedbackCommand request, CancellationToken cancellationToken)
    {
        var feedback = await _context.Feedbacks
            .FirstOrDefaultAsync(f => f.Id == request.FeedbackId, cancellationToken)
            ?? throw new KeyNotFoundException("Phản hồi không tồn tại.");

        feedback.AdminReply = request.Reply;
        feedback.RepliedAt = DateTime.UtcNow;
        feedback.IsRead = true;

        _context.Feedbacks.Update(feedback);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
