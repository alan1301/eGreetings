using EGreetings.Application.Commands.Subscribe.CreateSubscription;
using EGreetings.Application.Commands.Subscribe.RenewSubscription;
using EGreetings.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EGreetings.API.Controllers;

/// <summary>UC08, UC25, UC26 – Subscribe service management.</summary>
[ApiController]
[Route("api/subscriptions")]
[Authorize]
[Produces("application/json")]
public class SubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubscriptionsController(IMediator mediator) => _mediator = mediator;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>UC08 – Register subscribe service</summary>
    [HttpPost]
    public async Task<IActionResult> Subscribe([FromBody] CreateSubscriptionCommand command, CancellationToken ct)
    {
        var cmd = command with { UserId = CurrentUserId };
        var result = await _mediator.Send(cmd, ct);
        return Created($"/api/subscriptions/{result.SubscriptionId}",
            ApiResponse<CreateSubscriptionResult>.Created(result,
                "Đăng ký thành công. Vui lòng hoàn tất thanh toán."));
    }

    /// <summary>UC26 – Renew subscription</summary>
    [HttpPost("{id:guid}/renew")]
    public async Task<IActionResult> Renew(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new RenewSubscriptionCommand(id, CurrentUserId), ct);
        return Ok(ApiResponse<RenewSubscriptionResult>.Ok(result, result.Message));
    }
}
