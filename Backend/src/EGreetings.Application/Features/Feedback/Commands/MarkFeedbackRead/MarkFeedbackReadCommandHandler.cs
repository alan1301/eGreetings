using EGreetings.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Feedback.Commands.MarkFeedbackRead;

public class MarkFeedbackReadCommandHandler : IRequestHandler<MarkFeedbackReadCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public MarkFeedbackReadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(MarkFeedbackReadCommand request, CancellationToken cancellationToken)
    {
        var feedback = await _context.Feedbacks
            .FirstOrDefaultAsync(f => f.Id == request.FeedbackId, cancellationToken)
            ?? throw new KeyNotFoundException("Phản hồi không tồn tại.");

        feedback.IsRead = true;
        _context.Feedbacks.Update(feedback);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
