using MediatR;

namespace EGreetings.Subscription.Application.Features.Admin;

public record ConfirmPaymentCommand(
    int PaymentId,
    int AdminUserId,
    string? TransactionCode
) : IRequest<bool>;
