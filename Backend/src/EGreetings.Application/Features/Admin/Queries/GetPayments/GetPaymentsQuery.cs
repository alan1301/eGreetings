using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Admin.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Admin.Queries.GetPayments;

/// <summary>UC13 - Xem danh sách thanh toán (Admin)</summary>
public record GetPaymentsQuery(
    string? Status,
    int Page,
    int PageSize
) : IRequest<PagedResult<PaymentDto>>;
