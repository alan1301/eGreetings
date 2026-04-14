using EGreetings.Application.Features.Subscriptions.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Subscriptions.Commands.RenewSubscription;

/// <summary>UC26 - Gia hạn Subscribe - BR-30: Renew = +30 ngày từ ngày ExpiredAt</summary>
public record RenewSubscriptionCommand(int SubscriptionId, int UserId, string? PaymentMethod) : IRequest<SubscriptionDto>;
