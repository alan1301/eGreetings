using MediatR;

namespace EGreetings.Application.Features.Admin.Commands.ConfirmPayment;

/// <summary>UC13 - Xác nhận thanh toán từ Admin → Kích hoạt Subscription</summary>
public record ConfirmPaymentCommand(
    int PaymentId,
    int AdminUserId,
    string? TransactionCode
) : IRequest<bool>;
