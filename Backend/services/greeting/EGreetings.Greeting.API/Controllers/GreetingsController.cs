using EGreetings.Greeting.Application.Common.Models;
using EGreetings.Greeting.Application.DTOs;
using EGreetings.Greeting.Application.Features.Admin;
using EGreetings.Greeting.Application.Features.Greetings.Commands;
using EGreetings.Greeting.Application.Features.Greetings.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.Greeting.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GreetingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public GreetingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("send")]
    public async Task<ActionResult<SendGreetingResponse>> SendGreeting([FromBody] SendGreetingRequest request)
    {
        // Extract UserId from header if present
        var userIdStr = HttpContext.Request.Headers["X-UserId"].ToString();
        int? userId = string.IsNullOrWhiteSpace(userIdStr) ? null : int.Parse(userIdStr);

        var command = new SendGreetingCommand(
            userId,
            request.TemplateId,
            request.RecipientEmail,
            request.RecipientName,
            request.SenderMessage,
            request.CustomHtml,
            request.ScheduledAt
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<ActionResult<PagedResult<GreetingDto>>> GetUserGreetings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // Extract UserId from header
        var userIdStr = HttpContext.Request.Headers["X-UserId"].ToString();
        if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId))
        {
            return Unauthorized();
        }

        var query = new GetUserGreetingsQuery(userId, page, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("view/{token}")]
    public async Task<ActionResult<GreetingDto>> GetGreetingByToken(string token)
    {
        var query = new GetGreetingByTokenQuery(token);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("admin")]
    public async Task<ActionResult<PagedResult<GreetingDto>>> GetAllGreetings(
        [FromQuery] int? userId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // Check admin role from header
        if (!IsAdmin())
        {
            return Forbid();
        }

        var query = new GetAllGreetingsQuery(userId, status, page, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    private bool IsAdmin()
    {
        var userRole = HttpContext.Request.Headers["X-UserRole"].ToString();
        return userRole == "Admin";
    }
}

public record SendGreetingRequest(
    int TemplateId,
    string RecipientEmail,
    string RecipientName,
    string? SenderMessage,
    string? CustomHtml,
    DateTime? ScheduledAt
);
