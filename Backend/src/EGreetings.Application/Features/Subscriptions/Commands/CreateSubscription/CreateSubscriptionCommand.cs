using EGreetings.Application.Features.Subscriptions.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Subscriptions.Commands.CreateSubscription;

/// <summary>UC10 - Đăng ký Subscribe</summary>
public record CreateSubscriptionCommand(
    int UserId,
    decimal Price,
    int DurationDays,
    int MaxRecipients,
    string? PaymentMethod
) : IRequest<SubscriptionDto>;
