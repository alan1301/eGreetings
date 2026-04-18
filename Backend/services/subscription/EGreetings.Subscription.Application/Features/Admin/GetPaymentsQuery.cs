using MediatR;
using EGreetings.Subscription.Application.DTOs;

namespace EGreetings.Subscription.Application.Features.Admin;

public record GetPaymentsQuery(
    string? Status,
    int Page,
    int PageSize
) : IRequest<PagedResult<PaymentDto>>;
