using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Subscriptions.DTOs;
using EGreetings.Domain.Entities;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Subscriptions.Commands.RenewSubscription;

public class RenewSubscriptionCommandHandler : IRequestHandler<RenewSubscriptionCommand, SubscriptionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public RenewSubscriptionCommandHandler(IApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<SubscriptionDto> Handle(RenewSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Recipients)
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId && s.UserId == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Subscription không tồn tại.");

        if (subscription.Status == SubscriptionStatus.Disabled)
            throw new InvalidOperationException("Subscription đã bị vô hiệu hóa, không thể gia hạn.");

        // BR-30: Renew = +30 ngày từ ngày ExpiredAt (không phải từ hôm nay)
        var baseDate = subscription.ExpiredAt > DateTime.UtcNow
            ? subscription.ExpiredAt
            : DateTime.UtcNow;

        subscription.ExpiredAt = baseDate.AddDays(30);
        subscription.Status = SubscriptionStatus.Active;
        subscription.UpdatedAt = DateTime.UtcNow;

        // Tạo payment mới
        var payment = new Payment
        {
            UserId = request.UserId,
            SubscriptionId = subscription.Id,
            Amount = subscription.Price,
            PaymentMethod = request.PaymentMethod,
            Status = PaymentStatus.Pending
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(AuditEventType.SubscriptionRenewed, "Subscription",
            subscription.Id.ToString(),
            $"Subscription {subscription.Id} gia hạn đến {subscription.ExpiredAt:dd/MM/yyyy}",
            cancellationToken: cancellationToken);

        return new SubscriptionDto(subscription.Id, subscription.UserId,
            subscription.Status.ToString(), subscription.Price,
            subscription.StartDate, subscription.ExpiredAt,
            subscription.MaxRecipients, subscription.Recipients.Count(r => r.IsActive),
            subscription.CreatedAt);
    }
}
