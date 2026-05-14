using EGreetings.Application.Commands.Feedback.CreateFeedback;
using EGreetings.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EGreetings.API.Controllers;

/// <summary>UC07 – Create user feedback.</summary>
[ApiController]
[Route("api/feedbacks")]
[Authorize]
[Produces("application/json")]
public class FeedbacksController : ControllerBase
{
    private readonly IMediator _mediator;

    public FeedbacksController(IMediator mediator) => _mediator = mediator;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>UC07 – Submit feedback (BR-13: max 5/day)</summary>
    [HttpPost]
    public async Task<IActionResult> CreateFeedback([FromBody] CreateFeedbackCommand command, CancellationToken ct)
    {
        var cmd = command with { UserId = CurrentUserId };
        var id = await _mediator.Send(cmd, ct);
        return Created($"/api/feedbacks/{id}",
            ApiResponse<object>.Created(new { id }, "Thank you for your feedback!"));
    }
}
