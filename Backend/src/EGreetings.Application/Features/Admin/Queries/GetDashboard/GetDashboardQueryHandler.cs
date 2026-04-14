using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Admin.DTOs;
using EGreetings.Domain.Entities;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Admin.Queries.GetDashboard;

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardStatsDto>
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Greeting> _greetingRepo;
    private readonly IRepository<Subscription> _subscriptionRepo;
    private readonly IRepository<Payment> _paymentRepo;
    private readonly IRepository<GreetingTemplate> _templateRepo;
    private readonly IRepository<EGreetings.Domain.Entities.Feedback> _feedbackRepo;

    public GetDashboardQueryHandler(
        IRepository<User> userRepo,
        IRepository<Greeting> greetingRepo,
        IRepository<Subscription> subscriptionRepo,
        IRepository<Payment> paymentRepo,
        IRepository<GreetingTemplate> templateRepo,
        IRepository<EGreetings.Domain.Entities.Feedback> feedbackRepo)
    {
        _userRepo = userRepo;
        _greetingRepo = greetingRepo;
        _subscriptionRepo = subscriptionRepo;
        _paymentRepo = paymentRepo;
        _templateRepo = templateRepo;
        _feedbackRepo = feedbackRepo;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var totalUsers = await _userRepo.CountAsync(cancellationToken: cancellationToken);
        var activeUsers = await _userRepo.CountAsync(u => u.Status == UserStatus.Active, cancellationToken);
        var totalGreetings = await _greetingRepo.CountAsync(cancellationToken: cancellationToken);
        var sentGreetings = await _greetingRepo.CountAsync(g => g.Status == GreetingStatus.Sent, cancellationToken);
        var activeSubscriptions = await _subscriptionRepo.CountAsync(s => s.Status == SubscriptionStatus.Active, cancellationToken);
        var totalRevenue = await _paymentRepo.Query()
            .Where(p => p.Status == PaymentStatus.Paid)
            .SumAsync(p => p.Amount, cancellationToken);
        var totalTemplates = await _templateRepo.CountAsync(t => t.IsActive, cancellationToken);
        var unreadFeedbacks = await _feedbackRepo.CountAsync(f => !f.IsRead, cancellationToken);

        return new DashboardStatsDto(
            totalUsers, activeUsers,
            totalGreetings, sentGreetings,
            activeSubscriptions, totalRevenue,
            totalTemplates, unreadFeedbacks);
    }
}
