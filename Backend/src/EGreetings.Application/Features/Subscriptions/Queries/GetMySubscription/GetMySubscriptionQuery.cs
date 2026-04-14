using EGreetings.Application.Features.Subscriptions.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Subscriptions.Queries.GetMySubscription;

/// <summary>UC11 - Xem gói Subscribe của tôi</summary>
public record GetMySubscriptionQuery(int UserId) : IRequest<SubscriptionDetailDto?>;
