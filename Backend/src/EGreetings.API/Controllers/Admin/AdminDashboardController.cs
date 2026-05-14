using EGreetings.Application.Queries.Admin.Dashboard;
using EGreetings.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.API.Controllers.Admin;

/// <summary>Admin dashboard – overview, timeseries, funnel, top-cards, activity, health.</summary>
[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminDashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminDashboardController(IMediator mediator) => _mediator = mediator;

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDashboardOverviewQuery(), ct);
        return Ok(ApiResponse<DashboardOverviewDto>.Ok(result));
    }

    [HttpGet("timeseries")]
    public async Task<IActionResult> GetTimeseries(
        [FromQuery] string range = "7d", CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetDashboardTimeseriesQuery(range), ct);
        return Ok(ApiResponse<TimeseriesDto>.Ok(result));
    }

    [HttpGet("funnel")]
    public async Task<IActionResult> GetFunnel(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSubscriptionFunnelQuery(), ct);
        return Ok(ApiResponse<FunnelDto>.Ok(result));
    }

    [HttpGet("top-cards")]
    public async Task<IActionResult> GetTopCards(
        [FromQuery] string range = "7d", [FromQuery] int take = 5,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetTopCardsQuery(range, take), ct);
        return Ok(ApiResponse<IReadOnlyList<TopCardDto>>.Ok(result));
    }

    [HttpGet("activity")]
    public async Task<IActionResult> GetActivity(
        [FromQuery] int take = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetRecentActivityQuery(take), ct);
        return Ok(ApiResponse<IReadOnlyList<ActivityItemDto>>.Ok(result));
    }

    [HttpGet("health")]
    public async Task<IActionResult> GetHealth(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSystemHealthQuery(), ct);
        return Ok(ApiResponse<HealthDto>.Ok(result));
    }
}
