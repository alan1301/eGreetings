using MediatR;
using EGreetings.Subscription.Application.DTOs;

namespace EGreetings.Subscription.Application.Features.Subscriptions.Commands;

public record CreateSubscriptionCommand(
    int UserId,
    int PlanId,
    string PaymentMethod
) : IRequest<CreateSubscriptionResponse>;
