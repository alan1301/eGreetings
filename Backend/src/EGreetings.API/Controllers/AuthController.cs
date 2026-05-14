using EGreetings.Application.Commands.Auth.Login;
using EGreetings.Application.Commands.Auth.Logout;
using EGreetings.Application.Commands.Auth.ForgotPassword;
using EGreetings.Application.Commands.Auth.ResetPassword;
using EGreetings.Application.Commands.Auth.RegisterUser;
using EGreetings.Application.Commands.Auth.UpdateProfile;
using EGreetings.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EGreetings.API.Controllers;

/// <summary>
/// UC01 Register, UC02 Login, UC18 Logout, UC22 Forgot/Reset Password.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>UC01 – Register new user account</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<RegisterUserResult>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Created($"/api/users/{result.UserId}", ApiResponse<RegisterUserResult>.Created(result, result.Message));
    }

    /// <summary>UC02 – Login and receive JWT</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResult>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 422)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        // BR-05: Set httpOnly cookie for refresh token
        Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = command.RememberMe
                ? DateTimeOffset.UtcNow.AddDays(30)
                : DateTimeOffset.UtcNow.AddDays(1)
        });

        return Ok(ApiResponse<LoginResult>.Ok(result));
    }

    /// <summary>UC18 – Logout and revoke refresh token</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _mediator.Send(new LogoutCommand(userId), ct);

        Response.Cookies.Delete("refreshToken");
        return Ok(ApiResponse.Ok("Logged out successfully."));
    }

    /// <summary>UC22 – Request password reset link</summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        // UC22-E1: always return success to prevent email enumeration
        return Ok(ApiResponse.Ok("If the email exists, a password reset link has been sent."));
    }

    /// <summary>UC22 – Set new password using reset token</summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 422)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return Ok(ApiResponse.Ok("Password reset successfully. Please log in again."));
    }

    /// <summary>UC01 Step 2 – Verify email after registration (BR-03)</summary>
    [HttpGet("verify-email")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 422)]
    public async Task<IActionResult> VerifyEmail(
        [FromQuery] Guid userId, [FromQuery] string token, CancellationToken ct)
    {
        await _mediator.Send(new VerifyEmailCommand(userId, token), ct);
        return Ok(ApiResponse.Ok("Email verified successfully. You can now log in."));
    }

    /// <summary>UC19 – Update user profile and/or change password</summary>
    [HttpPut("me/profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 422)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var cmd = command with { UserId = userId };
        await _mediator.Send(cmd, ct);
        return Ok(ApiResponse.Ok("Profile updated successfully."));
    }
}
