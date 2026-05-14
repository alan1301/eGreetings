using EGreetings.Application.Commands.Admin.Subscriptions;
using EGreetings.Application.Queries.Admin;
using EGreetings.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.API.Controllers.Admin;

/// <summary>UC13 / UC14 – Admin subscription management.</summary>
[ApiController]
[Route("api/admin/subscriptions")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminSubscriptionsController : AuthorizedControllerBase
{
    private readonly IMediator _mediator;

    public AdminSubscriptionsController(IMediator mediator) => _mediator = mediator;

    // UC13: Get subscriptions
    [HttpGet]
    public async Task<IActionResult> GetSubscriptions(
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAdminSubscriptionsQuery(status, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<AdminSubscriptionDto>>.OkPaged(result, result.Meta));
    }

    // UC13: Activate subscription
    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> ActivateSubscription(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ActivateSubscriptionCommand(id, CurrentUserId), ct);
        return Ok(ApiResponse.Ok("Subscription activated."));
    }

    // Reject pending subscription
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> RejectSubscription(
        Guid id, [FromBody] RejectRequest req, CancellationToken ct)
    {
        await _mediator.Send(new RejectSubscriptionCommand(id, CurrentUserId, req.Reason), ct);
        return Ok(ApiResponse.Ok("Subscription rejected."));
    }

    // UC14: Disable subscription
    [HttpPost("{id:guid}/disable")]
    public async Task<IActionResult> DisableSubscription(
        Guid id, [FromBody] DisableRequest req, CancellationToken ct)
    {
        await _mediator.Send(new DisableSubscriptionCommand(id, CurrentUserId, req.Reason), ct);
        return Ok(ApiResponse.Ok("Subscription disabled."));
    }
}

public record DisableRequest(string Reason);
public record RejectRequest(string Reason);
