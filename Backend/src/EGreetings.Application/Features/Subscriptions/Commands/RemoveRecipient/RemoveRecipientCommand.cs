using MediatR;

namespace EGreetings.Application.Features.Subscriptions.Commands.RemoveRecipient;

/// <summary>UC11 - Xoá người nhận</summary>
public record RemoveRecipientCommand(
    int UserId,
    int SubscriptionId,
    int RecipientId
) : IRequest<bool>;
