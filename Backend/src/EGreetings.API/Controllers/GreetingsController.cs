using EGreetings.Application.Commands.Cards.CancelScheduledGreeting;
using EGreetings.Application.Commands.Cards.ScheduleGreeting;
using EGreetings.Application.Commands.Cards.SendGreeting;
using EGreetings.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.API.Controllers;

/// <summary>
/// UC06 – Send / schedule / cancel greeting cards.
/// </summary>
[ApiController]
[Route("api/greetings")]
[Authorize]
[Produces("application/json")]
public class GreetingsController : AuthorizedControllerBase
{
    private readonly IMediator _mediator;

    public GreetingsController(IMediator mediator) => _mediator = mediator;

    // UC06 – Send greeting card
    [HttpPost]
    public async Task<IActionResult> SendGreeting([FromBody] SendGreetingCardCommand command, CancellationToken ct)
    {
        var cmd = command with { SenderId = CurrentUserId, SenderEmail = CurrentUserEmail };
        var txId = await _mediator.Send(cmd, ct);
        return Ok(ApiResponse<object>.Ok(new { transactionId = txId }, "Greeting card sent successfully!"));
    }

    // UC06 Alt – Schedule greeting
    [HttpPost("schedule")]
    public async Task<IActionResult> ScheduleGreeting([FromBody] ScheduleGreetingCardCommand command, CancellationToken ct)
    {
        var cmd = command with { SenderId = CurrentUserId, SenderEmail = CurrentUserEmail };
        var txId = await _mediator.Send(cmd, ct);
        return Accepted($"/api/greetings/{txId}", ApiResponse<object>.Ok(
            new { transactionId = txId }, "Greeting card will be sent as scheduled."));
    }

    // UC06 – Cancel a scheduled greeting
    [HttpDelete("{id:guid}/schedule")]
    public async Task<IActionResult> CancelScheduled(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new CancelScheduledGreetingCommand(id, CurrentUserId), ct);
        return Ok(ApiResponse.Ok("Scheduled greeting cancelled."));
    }
}
