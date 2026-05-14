using EGreetings.Application.Queries.Admin;
using EGreetings.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.API.Controllers.Admin;

/// <summary>UC30 – System logs.</summary>
[ApiController]
[Route("api/admin/logs")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminLogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminLogsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetSystemLogs(
        [FromQuery] string? eventType, [FromQuery] string? status,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetSystemLogsQuery(eventType, status, from, to, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<SystemLogDto>>.OkPaged(result, result.Meta));
    }
}
