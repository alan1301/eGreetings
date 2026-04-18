using EGreetings.Identity.Application.DTOs;
using EGreetings.Identity.Application.Features.Auth.Commands.ChangePassword;
using EGreetings.Identity.Application.Features.Auth.Commands.ForgotPassword;
using EGreetings.Identity.Application.Features.Auth.Commands.Login;
using EGreetings.Identity.Application.Features.Auth.Commands.Register;
using EGreetings.Identity.Application.Features.Auth.Commands.ResetPassword;
using EGreetings.Identity.Application.Features.Auth.Commands.VerifyEmail;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.Identity.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new RegisterCommand
            {
                FullName = request.FullName,
                Email = request.Email,
                Password = request.Password,
                ConfirmPassword = request.ConfirmPassword,
                Phone = request.Phone
            };

            var userId = await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("User registered successfully. UserId: {UserId}, Email: {Email}", userId, request.Email);
            return Ok(new { userId });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Registration failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during registration");
            return StatusCode(500, new { error = "An internal error occurred" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new LoginCommand
            {
                Email = request.Email,
                Password = request.Password
            };

            var response = await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("User logged in successfully. UserId: {UserId}, Email: {Email}", response.UserId, request.Email);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Login failed: {Message}", ex.Message);
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during login");
            return StatusCode(500, new { error = "An internal error occurred" });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        _logger.LogInformation("User logged out successfully");
        return Ok(new { message = "Logout successful" });
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token, CancellationToken cancellationToken)
    {
        try
        {
            var command = new VerifyEmailCommand { Token = token };
            await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("Email verified successfully");
            return Ok(new { message = "Email verified successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Email verification failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during email verification");
            return StatusCode(500, new { error = "An internal error occurred" });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new ForgotPasswordCommand { Email = request.Email };
            await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("Password reset email sent (or silently handled for: {Email})", request.Email);
            return Ok(new { message = "If an account exists, a password reset email has been sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during forgot password");
            return StatusCode(500, new { error = "An internal error occurred" });
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new ResetPasswordCommand
            {
                Token = request.Token,
                NewPassword = request.NewPassword,
                ConfirmPassword = request.ConfirmPassword
            };

            await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("Password reset successfully");
            return Ok(new { message = "Password reset successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Password reset failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during password reset");
            return StatusCode(500, new { error = "An internal error occurred" });
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userIdHeader = Request.Headers["X-UserId"].ToString();
            if (!int.TryParse(userIdHeader, out var userId))
            {
                _logger.LogWarning("Invalid X-UserId header");
                return BadRequest(new { error = "Invalid user context" });
            }

            var command = new ChangePasswordCommand
            {
                UserId = userId,
                CurrentPassword = request.CurrentPassword,
                NewPassword = request.NewPassword,
                ConfirmPassword = request.ConfirmPassword
            };

            await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("Password changed successfully for UserId: {UserId}", userId);
            return Ok(new { message = "Password changed successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Change password failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during password change");
            return StatusCode(500, new { error = "An internal error occurred" });
        }
    }
}
