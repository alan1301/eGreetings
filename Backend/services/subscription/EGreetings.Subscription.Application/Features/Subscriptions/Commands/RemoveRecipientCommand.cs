using MediatR;

namespace EGreetings.Subscription.Application.Features.Subscriptions.Commands;

public record RemoveRecipientCommand(
    int UserId,
    int SubscriptionId,
    int RecipientId
) : IRequest<bool>;
