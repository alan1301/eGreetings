using MediatR;
using EGreetings.Subscription.Application.DTOs;

namespace EGreetings.Subscription.Application.Features.Subscriptions.Commands;

public record RenewSubscriptionCommand(
    int SubscriptionId,
    int UserId,
    string? PaymentMethod
) : IRequest<CreateSubscriptionResponse>;
