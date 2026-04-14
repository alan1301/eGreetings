using EGreetings.Application.Common.Interfaces;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Admin.Commands.ConfirmPayment;

public class ConfirmPaymentCommandHandler : IRequestHandler<ConfirmPaymentCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public ConfirmPaymentCommandHandler(IApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<bool> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .Include(p => p.Subscription)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken)
            ?? throw new KeyNotFoundException("Thanh toán không tồn tại.");

        if (payment.Subscription == null)
            throw new InvalidOperationException("Thanh toán này không liên kết với gói Subscribe.");

        payment.Status = PaymentStatus.Paid;
        payment.PaidAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.TransactionCode))
            payment.TransactionCode = request.TransactionCode;

        payment.Subscription.Status = SubscriptionStatus.Active;

        _context.Payments.Update(payment);
        _context.Subscriptions.Update(payment.Subscription);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(AuditEventType.PaymentCompleted, "Payment", payment.Id.ToString(),
            $"Admin xác nhận thanh toán {payment.TransactionCode}. Gói Subscribe {payment.SubscriptionId} kích hoạt.",
            cancellationToken: cancellationToken);

        return true;
    }
}
