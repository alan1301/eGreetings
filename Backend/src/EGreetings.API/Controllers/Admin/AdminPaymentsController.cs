using EGreetings.Application.Queries.Admin;
using EGreetings.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.API.Controllers.Admin;

/// <summary>UC12 – Payment transactions history and greeting transaction report.</summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminPaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminPaymentsController(IMediator mediator) => _mediator = mediator;

    // Payment Transactions History
    [HttpGet("payments")]
    public async Task<IActionResult> GetPaymentTransactions(
        [FromQuery] string? status, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetPaymentTransactionsQuery(status, search, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<PaymentTransactionAdminDto>>.OkPaged(result, result.Meta));
    }

    // UC12: Greeting transaction report
    [HttpGet("reports/transactions")]
    public async Task<IActionResult> GetTransactionReport(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetTransactionReportQuery(from, to, search, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<TransactionDto>>.OkPaged(result, result.Meta));
    }
}
