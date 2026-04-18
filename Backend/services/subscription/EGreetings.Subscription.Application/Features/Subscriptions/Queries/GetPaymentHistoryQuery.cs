using MediatR;
using EGreetings.Subscription.Application.DTOs;

namespace EGreetings.Subscription.Application.Features.Subscriptions.Queries;

public record GetPaymentHistoryQuery(int UserId) : IRequest<List<PaymentDto>>;
