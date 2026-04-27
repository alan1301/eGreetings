using EGreetings.Application.Interfaces;
using EGreetings.Domain.Entities;
using EGreetings.Domain.Exceptions;
using EGreetings.Shared.Constants;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Commands.Feedback.CreateFeedback;

/// <summary>UC07 – BR-13: Max 5 feedbacks per user per day.</summary>
public record CreateFeedbackCommand(
    Guid UserId,
    string Title,
    string Content,
    int? StarRating
) : IRequest<Guid>;

public class CreateFeedbackCommandValidator : AbstractValidator<CreateFeedbackCommand>
{
    public CreateFeedbackCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.StarRating)
            .InclusiveBetween(1, 5).When(x => x.StarRating.HasValue)
            .WithMessage("Đánh giá sao phải từ 1 đến 5");
    }
}

public class CreateFeedbackCommandHandler : IRequestHandler<CreateFeedbackCommand, Guid>
{
    private readonly IAppDbContext _db;

    public CreateFeedbackCommandHandler(IAppDbContext db) => _db = db;

    public async Task<Guid> Handle(CreateFeedbackCommand request, CancellationToken ct)
    {
        // BR-13: Max 5 feedbacks per user per day
        var today = DateTime.UtcNow.Date;
        var todayCount = await _db.Feedbacks.CountAsync(
            f => f.UserId == request.UserId
              && f.CreatedAt >= today && f.CreatedAt < today.AddDays(1), ct);

        if (todayCount >= BusinessConstants.MaxFeedbacksPerUserPerDay)
            throw new BusinessRuleViolationException("BR-13",
                $"Đã gửi {BusinessConstants.MaxFeedbacksPerUserPerDay} phản hồi hôm nay. Vui lòng thử lại ngày mai.");

        var feedback = new EGreetings.Domain.Entities.Feedback
        {
            UserId = request.UserId,
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            StarRating = request.StarRating
        };

        await _db.Feedbacks.AddAsync(feedback, ct);
        await _db.SaveChangesAsync(ct);

        return feedback.Id;
    }
}
