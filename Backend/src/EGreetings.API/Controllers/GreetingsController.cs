using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Greetings.Commands.SendGreeting;
using EGreetings.Application.Features.Greetings.Queries.GetGreetingByToken;
using EGreetings.Application.Features.Greetings.Queries.GetUserGreetings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.API.Controllers;

/// <summary>
/// UC05/UC06 - Tùy chỉnh và gửi thiệp | UC07 - Xem thiệp đã gửi
/// UC08 - Guest checkout | UC09 - Xem thiệp (người nhận)
/// UC24 - Xem chi tiết thiệp (Guest)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GreetingsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public GreetingsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>UC09/UC24 - Xem thiệp qua link token (Public - cho người nhận + Guest)</summary>
    [HttpGet("view/{token}")]
    public async Task<IActionResult> ViewGreeting(string token, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetGreetingByTokenQuery(token), ct);
        return Ok(new { Success = true, Data = result });
    }

    /// <summary>UC07 - Xem danh sách thiệp đã gửi (Authenticated User)</summary>
    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyGreetings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Vui lòng đăng nhập.");

        var result = await _mediator.Send(new GetUserGreetingsQuery(userId, page, pageSize), ct);
        return Ok(new { Success = true, Data = result });
    }

    /// <summary>
    /// UC06 - Gửi thiệp (User đã đăng nhập)
    /// UC08 - Guest checkout (gửi thiệp không cần đăng nhập, chỉ mẫu miễn phí)
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendGreeting([FromBody] SendGreetingCommand command, CancellationToken ct)
    {
        // Nếu đã đăng nhập thì gắn UserId
        var userId = _currentUser.IsAuthenticated ? _currentUser.UserId : null;

        var cmd = command with { UserId = userId };
        var result = await _mediator.Send(cmd, ct);

        return Ok(new
        {
            Success = true,
            Data = result,
            Message = result.ScheduledAt.HasValue
                ? $"Thiệp đã được lên lịch gửi lúc {result.ScheduledAt:dd/MM/yyyy HH:mm}."
                : "Thiệp đã được gửi thành công!"
        });
    }
}
