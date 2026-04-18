using MediatR;
using EGreetings.Subscription.Application.DTOs;

namespace EGreetings.Subscription.Application.Features.Admin;

public record PagedResult<T>(
    List<T> Items,
    int Total,
    int Page,
    int PageSize
);

public record GetAllSubscriptionsQuery(
    string? Status,
    int? UserId,
    int Page,
    int PageSize
) : IRequest<PagedResult<SubscriptionDto>>;
