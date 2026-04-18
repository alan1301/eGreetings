using EGreetings.Identity.Application.DTOs;
using EGreetings.Identity.Application.Features.Auth.Commands.BanUser;
using EGreetings.Identity.Application.Features.Auth.Queries.GetUsers;
using EGreetings.Identity.Domain.Enums;
using EGreetings.Shared.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.Identity.API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IMediator mediator, ILogger<UsersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var userRoleHeader = Request.Headers["X-UserRole"].ToString();
            if (userRoleHeader != UserRole.Admin)
            {
                _logger.LogWarning("Non-admin user attempted to access users list");
                return Forbid();
            }

            var query = new GetUsersQuery
            {
                Search = search,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query, cancellationToken);
            _logger.LogInformation("Users list retrieved. Page: {Page}, Total: {Total}", page, result.Total);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving users");
            return StatusCode(500, new { error = "An internal error occurred" });
        }
    }

    [HttpPost("{id}/ban")]
    public async Task<IActionResult> BanUser(int id, [FromBody] BanUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userRoleHeader = Request.Headers["X-UserRole"].ToString();
            if (userRoleHeader != UserRole.Admin)
            {
                _logger.LogWarning("Non-admin user attempted to ban user");
                return Forbid();
            }

            var adminIdHeader = Request.Headers["X-UserId"].ToString();
            if (!int.TryParse(adminIdHeader, out var adminId))
            {
                _logger.LogWarning("Invalid X-UserId header");
                return BadRequest(new { error = "Invalid user context" });
            }

            var command = new BanUserCommand
            {
                TargetUserId = id,
                IsBanning = request.IsBanning,
                Reason = request.Reason,
                AdminUserId = adminId
            };

            await _mediator.Send(command, cancellationToken);
            var action = request.IsBanning ? "banned" : "unbanned";
            _logger.LogInformation("User {UserId} has been {Action} by admin {AdminId}", id, action, adminId);
            return Ok(new { message = $"User {action} successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Ban operation failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during ban operation");
            return StatusCode(500, new { error = "An internal error occurred" });
        }
    }
}

public class BanUserRequest
{
    public bool IsBanning { get; set; }
    public string? Reason { get; set; }
}
