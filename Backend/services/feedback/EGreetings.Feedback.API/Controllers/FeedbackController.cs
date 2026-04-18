using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EGreetings.Feedback.Application.DTOs;
using EGreetings.Feedback.Application.Features;

namespace EGreetings.Feedback.API.Controllers;

[ApiController]
[Route("api/feedback")]
public class FeedbackController : ControllerBase
{
    private readonly IMediator _mediator;

    public FeedbackController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private int GetUserId()
    {
        var userIdHeader = HttpContext.Request.Headers["X-UserId"].ToString();
        return int.TryParse(userIdHeader, out var userId) ? userId : 0;
    }

    private bool IsAdmin()
    {
        var roleHeader = HttpContext.Request.Headers["X-UserRole"].ToString();
        return roleHeader.Equals("Admin", StringComparison.OrdinalIgnoreCase);
    }

    [HttpPost]
    public async Task<ActionResult<int>> SubmitFeedback([FromBody] SubmitFeedbackCommand command)
    {
        var userId = GetUserId();
        var commandWithUserId = userId > 0
            ? new SubmitFeedbackCommand(userId, command.Subject, command.Content, command.ContactEmail, command.StarRating)
            : command;

        var result = await _mediator.Send(commandWithUserId);
        return CreatedAtAction(nameof(SubmitFeedback), result);
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<PagedResult<FeedbackDto>>> GetFeedbacks(
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (!IsAdmin()) return Forbid();

        var result = await _mediator.Send(new GetFeedbacksQuery(status, page, pageSize));
        return Ok(result);
    }

    [HttpPost("{id}/reply")]
    [Authorize]
    public async Task<IActionResult> ReplyFeedback(int id, [FromBody] ReplyFeedbackCommand command)
    {
        if (!IsAdmin()) return Forbid();

        var adminId = GetUserId();
        if (adminId <= 0) return Unauthorized();

        await _mediator.Send(new ReplyFeedbackCommand(id, adminId, command.Reply));
        return NoContent();
    }

    [HttpPatch("{id}/read")]
    [Authorize]
    public async Task<IActionResult> MarkFeedbackRead(int id)
    {
        if (!IsAdmin()) return Forbid();

        await _mediator.Send(new MarkFeedbackReadCommand(id));
        return NoContent();
    }
}
