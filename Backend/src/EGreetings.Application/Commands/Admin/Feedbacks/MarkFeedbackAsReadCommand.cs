using EGreetings.Application.Interfaces;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Commands.Admin.Feedbacks;

public record MarkFeedbackAsReadCommand(Guid FeedbackId) : IRequest<bool>;

public class MarkFeedbackAsReadCommandHandler : IRequestHandler<MarkFeedbackAsReadCommand, bool>
{
    private readonly IAppDbContext _db;

    public MarkFeedbackAsReadCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(MarkFeedbackAsReadCommand request, CancellationToken ct)
    {
        var feedback = await _db.Feedbacks.FirstOrDefaultAsync(f => f.Id == request.FeedbackId, ct);
        if (feedback == null)
            throw new KeyNotFoundException("Feedback not found");

        feedback.Status = FeedbackStatus.Read; // Mark as read

        await _db.SaveChangesAsync(ct);
        return true;
    }
}
