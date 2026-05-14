using EGreetings.Application.Commands.Admin.Feedbacks;
using EGreetings.Application.Queries.Admin;
using EGreetings.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.API.Controllers.Admin;

/// <summary>UC11 – Admin feedback management.</summary>
[ApiController]
[Route("api/admin/feedbacks")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminFeedbacksController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminFeedbacksController(IMediator mediator) => _mediator = mediator;

    // UC11: Get feedbacks
    [HttpGet]
    public async Task<IActionResult> GetFeedbacks(
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetFeedbacksQuery(status, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<FeedbackDto>>.OkPaged(result, result.Meta));
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkFeedbackAsRead(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new MarkFeedbackAsReadCommand(id), ct);
        return Ok(ApiResponse.Ok("Feedback marked as read."));
    }
}
