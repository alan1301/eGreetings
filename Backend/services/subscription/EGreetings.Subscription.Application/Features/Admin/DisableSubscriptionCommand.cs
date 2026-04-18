using MediatR;

namespace EGreetings.Subscription.Application.Features.Admin;

public record DisableSubscriptionCommand(
    int SubscriptionId,
    int AdminUserId,
    string? Reason
) : IRequest<bool>;
