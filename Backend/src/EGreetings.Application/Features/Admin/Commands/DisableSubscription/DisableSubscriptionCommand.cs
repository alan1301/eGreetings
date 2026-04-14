using MediatR;

namespace EGreetings.Application.Features.Admin.Commands.DisableSubscription;

/// <summary>UC21 - Vô hiệu hóa gói Subscribe</summary>
public record DisableSubscriptionCommand(
    int SubscriptionId,
    int AdminUserId,
    string? Reason
) : IRequest<bool>;
