using EGreetings.Application.Features.Auth.Commands.ForgotPassword;
using EGreetings.Application.Features.Auth.Commands.Login;
using EGreetings.Application.Features.Auth.Commands.Register;
using EGreetings.Application.Features.Auth.Commands.ResetPassword;
using EGreetings.Application.Features.Auth.Commands.VerifyEmail;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.API.Controllers;

/// <summary>
/// UC01 - Đăng ký | UC02 - Đăng nhập | UC18 - Đăng xuất | UC22 - Quên mật khẩu
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>UC01 - Đăng ký tài khoản mới</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(new { Success = true, Data = result, Message = "Đăng ký thành công! Vui lòng kiểm tra email để xác thực tài khoản." });
    }

    /// <summary>UC02 - Đăng nhập (User + Admin)</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(new { Success = true, Data = result, Message = "Đăng nhập thành công." });
    }

    /// <summary>UC18 - Đăng xuất (Client-side token invalidation)</summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // JWT là stateless; client xóa token phía client
        // Nếu cần blacklist token, implement Redis-based token blacklist
        return Ok(new { Success = true, Message = "Đăng xuất thành công." });
    }

    /// <summary>UC22 - Quên mật khẩu (gửi link reset)</summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return Ok(new { Success = true, Message = "Nếu email tồn tại, chúng tôi đã gửi link đặt lại mật khẩu." });
    }

    /// <summary>UC22 - Đặt lại mật khẩu bằng token</summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return Ok(new { Success = true, Message = "Mật khẩu đã được đặt lại thành công." });
    }

    /// <summary>UC01 - Xác thực email qua token</summary>
    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token, CancellationToken ct)
    {
        await _mediator.Send(new VerifyEmailCommand(token), ct);
        return Ok(new { Success = true, Message = "Email đã được xác thực thành công." });
    }
}
