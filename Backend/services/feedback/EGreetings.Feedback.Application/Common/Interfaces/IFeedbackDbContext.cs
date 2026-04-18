using EGreetings.Feedback.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Feedback.Application.Common.Interfaces;

public interface IFeedbackDbContext
{
    DbSet<global::EGreetings.Feedback.Domain.Entities.Feedback> Feedbacks { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
