using EGreetings.Admin.Application.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace EGreetings.Admin.Application.Features.Dashboard;

public record GetDashboardQuery : IRequest<DashboardDto>;

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GetDashboardQueryHandler> _logger;

    public GetDashboardQueryHandler(IHttpClientFactory httpClientFactory, ILogger<GetDashboardQueryHandler> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var totalUsers = await GetTotalUsersAsync(cancellationToken);
            var activeSubscriptions = await GetActiveSubscriptionsAsync(cancellationToken);
            var pendingPayments = await GetPendingPaymentsAsync(cancellationToken);
            var greetingsSentToday = await GetGreetingsSentTodayAsync(cancellationToken);
            var unreadFeedbacks = await GetUnreadFeedbacksAsync(cancellationToken);
            var revenueThisMonth = await GetRevenueThisMonthAsync(cancellationToken);

            return new DashboardDto(
                totalUsers,
                activeSubscriptions,
                pendingPayments,
                greetingsSentToday,
                unreadFeedbacks,
                revenueThisMonth);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard data");
            throw;
        }
    }

    private async Task<int> GetTotalUsersAsync(CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("IdentityService");
            var response = await client.GetAsync("/internal/users/count", ct);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                return int.Parse(content);
            }
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch TotalUsers from Identity Service");
            return 0;
        }
    }

    private async Task<int> GetActiveSubscriptionsAsync(CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("SubscriptionService");
            var response = await client.GetAsync("/internal/subscriptions/active/count", ct);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                return int.Parse(content);
            }
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch ActiveSubscriptions from Subscription Service");
            return 0;
        }
    }

    private async Task<int> GetPendingPaymentsAsync(CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("SubscriptionService");
            var response = await client.GetAsync("/internal/payments/pending/count", ct);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                return int.Parse(content);
            }
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch PendingPayments from Subscription Service");
            return 0;
        }
    }

    private async Task<int> GetGreetingsSentTodayAsync(CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("GreetingService");
            var response = await client.GetAsync("/internal/greetings/sent-today/count", ct);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                return int.Parse(content);
            }
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch GreetingsSentToday from Greeting Service");
            return 0;
        }
    }

    private async Task<int> GetUnreadFeedbacksAsync(CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FeedbackService");
            var response = await client.GetAsync("/internal/feedback/unread/count", ct);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                return int.Parse(content);
            }
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch UnreadFeedbacks from Feedback Service");
            return 0;
        }
    }

    private async Task<decimal> GetRevenueThisMonthAsync(CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("SubscriptionService");
            var response = await client.GetAsync("/internal/payments/revenue/this-month", ct);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                return decimal.Parse(content);
            }
            return 0m;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch RevenueThisMonth from Subscription Service");
            return 0m;
        }
    }
}
