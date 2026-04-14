using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Subscriptions.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Admin.Queries.GetSubscriptions;

/// <summary>UC21 - Xem danh sách gói Subscribe (Admin)</summary>
public record GetAllSubscriptionsQuery(
    string? Status,
    int? UserId,
    int Page,
    int PageSize
) : IRequest<PagedResult<SubscriptionDto>>;
