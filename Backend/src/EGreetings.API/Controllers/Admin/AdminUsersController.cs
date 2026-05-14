using EGreetings.Application.Commands.Admin.Subscriptions;
using EGreetings.Application.Commands.Admin.Users;
using EGreetings.Application.Queries.Admin;
using EGreetings.Domain.Enums;
using EGreetings.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.API.Controllers.Admin;

/// <summary>UC21 – Admin user management (CRUD + lock/unlock + grant subscription).</summary>
[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminUsersController : AuthorizedControllerBase
{
    private readonly IMediator _mediator;

    public AdminUsersController(IMediator mediator) => _mediator = mediator;

    // UC21: Get users
    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search,
        [FromQuery] string? role,
        [FromQuery] string? accountStatus,
        [FromQuery] string? subscriptionPlan,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetAdminUsersQuery(search, role, accountStatus, subscriptionPlan, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<AdminUserDto>>.OkPaged(result, result.Meta));
    }

    // Create user
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest req, CancellationToken ct)
    {
        var id = await _mediator.Send(
            new AdminCreateUserCommand(CurrentUserId, req.FullName, req.Email, req.Password, req.Role), ct);
        return Created($"/api/admin/users/{id}", ApiResponse<object>.Created(new { id }));
    }

    // Update user
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest req, CancellationToken ct)
    {
        await _mediator.Send(new AdminUpdateUserCommand(id, CurrentUserId, req.FullName, req.Email, req.Role), ct);
        return Ok(ApiResponse.Ok("User updated."));
    }

    // Delete user (soft)
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new AdminDeleteUserCommand(id, CurrentUserId), ct);
        return Ok(ApiResponse.Ok("User deleted."));
    }

    // UC21: Lock user
    [HttpPost("{id:guid}/lock")]
    public async Task<IActionResult> LockUser(
        Guid id, [FromBody] LockRequest req, CancellationToken ct)
    {
        await _mediator.Send(new LockUserCommand(id, CurrentUserId, req.Reason), ct);
        return Ok(ApiResponse.Ok("User account locked."));
    }

    // UC21: Unlock user
    [HttpPost("{id:guid}/unlock")]
    public async Task<IActionResult> UnlockUser(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new UnlockUserCommand(id), ct);
        return Ok(ApiResponse.Ok("User account unlocked."));
    }

    // Grant subscription to user
    [HttpPost("{userId:guid}/subscriptions")]
    public async Task<IActionResult> GrantSubscription(
        Guid userId, [FromBody] GrantSubscriptionRequest req, CancellationToken ct)
    {
        var id = await _mediator.Send(
            new AdminGrantSubscriptionCommand(userId, CurrentUserId, req.Plan, req.ExpiryDate, req.Notes), ct);
        return Created($"/api/admin/subscriptions/{id}",
            ApiResponse<object>.Created(new { id }));
    }
}

public record LockRequest(string Reason);
public record CreateUserRequest(string FullName, string Email, string Password, string Role);
public record UpdateUserRequest(string FullName, string Email, string Role);
public record GrantSubscriptionRequest(
    SubscriptionPlan Plan,
    DateTime? ExpiryDate = null,
    string? Notes = null
);
