using EGreetings.Application.Queries.Cards;
using EGreetings.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.API.Controllers;

/// <summary>
/// UC17 + Dashboard – User-scoped resources (drafts, history, upcoming events).
/// </summary>
[ApiController]
[Route("api/me")]
[Authorize]
[Produces("application/json")]
public class MeController : AuthorizedControllerBase
{
    private readonly IMediator _mediator;

    public MeController(IMediator mediator) => _mediator = mediator;

    // UC17 – My drafts
    [HttpGet("drafts")]
    public async Task<IActionResult> MyDrafts(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetDraftsQuery(CurrentUserId, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<DraftDto>>.OkPaged(result, result.Meta));
    }

    // UC17 – Greeting history
    [HttpGet("greeting-history")]
    public async Task<IActionResult> MyGreetingHistory(
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetGreetingHistoryQuery(CurrentUserId, status, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<GreetingHistoryDto>>.OkPaged(result, result.Meta));
    }

    // Dashboard – Unified upcoming events (Scheduled + Contact Occasions)
    [HttpGet("upcoming-events")]
    public async Task<IActionResult> GetUpcomingEvents(
        [FromQuery] int windowDays = 30, [FromQuery] int maxItems = 8,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetUpcomingEventsQuery(CurrentUserId, windowDays, maxItems), ct);
        return Ok(ApiResponse<List<UpcomingEventDto>>.Ok(result));
    }
}
