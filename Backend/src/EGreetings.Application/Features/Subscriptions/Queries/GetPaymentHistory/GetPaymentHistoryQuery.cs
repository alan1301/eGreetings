using EGreetings.Application.Features.Admin.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Subscriptions.Queries.GetPaymentHistory;

/// <summary>UC25 - Lịch sử thanh toán</summary>
public record GetPaymentHistoryQuery(int UserId) : IRequest<IReadOnlyList<PaymentDto>>;
