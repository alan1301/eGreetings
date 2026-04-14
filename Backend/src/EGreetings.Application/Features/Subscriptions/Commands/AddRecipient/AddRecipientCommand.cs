using MediatR;

namespace EGreetings.Application.Features.Subscriptions.Commands.AddRecipient;

/// <summary>UC11 - Thêm người nhận - BR-14, BR-20, BR-21</summary>
public record AddRecipientCommand(
    int UserId,
    int SubscriptionId,
    string Name,
    string Email,
    DateTime? Birthday
) : IRequest<int>;
