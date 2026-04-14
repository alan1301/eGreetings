using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Subscriptions.DTOs;
using EGreetings.Domain.Entities;
using EGreetings.Domain.Enums;
using MediatR;

namespace EGreetings.Application.Features.Subscriptions.Commands.CreateSubscription;

public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, SubscriptionDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _auditService;

    public CreateSubscriptionCommandHandler(IUnitOfWork uow, IAuditService auditService)
    {
        _uow = uow;
        _auditService = auditService;
    }

    public async Task<SubscriptionDto> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        // BR-14: Mỗi gói Subscribe phải có ít nhất 10 địa chỉ email người nhận
        if (request.MaxRecipients < 10)
            throw new InvalidOperationException("BR-14: Gói Subscribe phải có ít nhất 10 người nhận.");

        var now = DateTime.UtcNow;

        var subscription = new Subscription
        {
            UserId = request.UserId,
            Status = SubscriptionStatus.Pending,
            Price = request.Price,
            StartDate = now,
            ExpiredAt = now.AddDays(request.DurationDays),
            MaxRecipients = request.MaxRecipients
        };

        _uow.Repository<Subscription>().Add(subscription);

        var payment = new Payment
        {
            UserId = request.UserId,
            Subscription = subscription,
            Amount = request.Price,
            PaymentMethod = request.PaymentMethod,
            Status = PaymentStatus.Pending
        };

        _uow.Repository<Payment>().Add(payment);
        await _uow.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(AuditEventType.SubscriptionCreated, "Subscription",
            subscription.Id.ToString(),
            $"Subscription mới cho User {request.UserId}", cancellationToken: cancellationToken);

        return new SubscriptionDto(subscription.Id, subscription.UserId,
            subscription.Status.ToString(), subscription.Price,
            subscription.StartDate, subscription.ExpiredAt,
            subscription.MaxRecipients, 0, subscription.CreatedAt);
    }
}
