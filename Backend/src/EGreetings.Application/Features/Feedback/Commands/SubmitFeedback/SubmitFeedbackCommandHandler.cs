using EGreetings.Application.Common.Interfaces;
using EGreetings.Domain.Entities;
using MediatR;

namespace EGreetings.Application.Features.Feedback.Commands.SubmitFeedback;

public class SubmitFeedbackCommandHandler : IRequestHandler<SubmitFeedbackCommand, int>
{
    private readonly IUnitOfWork _uow;

    public SubmitFeedbackCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<int> Handle(SubmitFeedbackCommand request, CancellationToken cancellationToken)
    {
        // BR-13: Mỗi tài khoản chỉ được gửi tối đa 5 phản hồi/ngày
        if (request.UserId.HasValue)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);
            var countToday = await _uow.Repository<EGreetings.Domain.Entities.Feedback>()
                .CountAsync(f => f.UserId == request.UserId.Value
                    && f.CreatedAt >= today
                    && f.CreatedAt < tomorrow, cancellationToken);

            if (countToday >= 5)
                throw new InvalidOperationException("BR-13: Bạn đã gửi tối đa 5 phản hồi trong ngày hôm nay.");
        }

        // Validate StarRating range (1–5)
        if (request.StarRating.HasValue && (request.StarRating < 1 || request.StarRating > 5))
            throw new ArgumentOutOfRangeException(nameof(request.StarRating), "Đánh giá sao phải trong khoảng 1–5.");

        var feedback = new EGreetings.Domain.Entities.Feedback
        {
            UserId = request.UserId,
            Subject = request.Subject,
            Content = request.Content,
            ContactEmail = request.ContactEmail,
            StarRating = request.StarRating
        };

        _uow.Repository<EGreetings.Domain.Entities.Feedback>().Add(feedback);
        await _uow.SaveChangesAsync(cancellationToken);

        return feedback.Id;
    }
}
