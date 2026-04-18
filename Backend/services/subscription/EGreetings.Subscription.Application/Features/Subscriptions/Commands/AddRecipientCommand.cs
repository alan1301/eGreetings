using MediatR;

namespace EGreetings.Subscription.Application.Features.Subscriptions.Commands;

public record AddRecipientCommand(
    int UserId,
    int SubscriptionId,
    string Name,
    string Email,
    DateTime? Birthday
) : IRequest<int>;
