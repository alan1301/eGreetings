using EGreetings.Application.Commands.Admin.Cards;
using EGreetings.Application.Commands.Admin.Categories;
using EGreetings.Application.Commands.Admin.Subscriptions;
using EGreetings.Application.Commands.Admin.Users;
using EGreetings.Application.Queries.Admin;
using EGreetings.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EGreetings.API.Controllers.Admin;

/// <summary>Admin controllers – UC09, UC10, UC11, UC12, UC13, UC14, UC21, UC27, UC30</summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator) => _mediator = mediator;

    private Guid CurrentAdminId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ──────── UC09: Create card ──────────────────────────────────
    [HttpPost("cards")]
    public async Task<IActionResult> CreateCard([FromBody] CreateCardCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return Created($"/api/cards/{id}", ApiResponse<object>.Created(new { id }));
    }

    // ──────── UC09: Update card ──────────────────────────────────
    [HttpPut("cards/{id:guid}")]
    public async Task<IActionResult> UpdateCard(Guid id, [FromBody] UpdateCardCommand command, CancellationToken ct)
    {
        var cmd = command with { CardId = id };
        await _mediator.Send(cmd, ct);
        return Ok(ApiResponse.Ok("Đã cập nhật mẫu thiệp."));
    }

    // ──────── UC10: Archive card ─────────────────────────────────
    [HttpPatch("cards/{id:guid}/archive")]
    public async Task<IActionResult> ArchiveCard(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ArchiveCardCommand(id), ct);
        return Ok(ApiResponse.Ok("Đã ẩn mẫu thiệp."));
    }

    // ──────── UC10: Delete card (if no transactions) ─────────────
    [HttpDelete("cards/{id:guid}")]
    public async Task<IActionResult> DeleteCard(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteCardCommand(id), ct);
        return Ok(ApiResponse.Ok("Đã xóa mẫu thiệp."));
    }

    // ──────── UC27: Create category ──────────────────────────────
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return Created($"/api/categories/{id}", ApiResponse<object>.Created(new { id }));
    }

    // ──────── UC27: Hide category ─────────────────────────────────
    [HttpPatch("categories/{id:guid}/hide")]
    public async Task<IActionResult> HideCategory(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new HideCategoryCommand(id), ct);
        return Ok(ApiResponse.Ok("Đã ẩn danh mục."));
    }

    // ──────── UC13: Get subscriptions ────────────────────────
    [HttpGet("subscriptions")]
    public async Task<IActionResult> GetSubscriptions(
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAdminSubscriptionsQuery(status, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<AdminSubscriptionDto>>.OkPaged(result, result.Meta));
    }

    // ──────── UC13: Activate subscription ────────────────────────
    [HttpPost("subscriptions/{id:guid}/activate")]
    public async Task<IActionResult> ActivateSubscription(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ActivateSubscriptionCommand(id, CurrentAdminId), ct);
        return Ok(ApiResponse.Ok("Đã kích hoạt dịch vụ Subscribe."));
    }

    // ──────── UC14: Disable subscription ─────────────────────────
    [HttpPost("subscriptions/{id:guid}/disable")]
    public async Task<IActionResult> DisableSubscription(
        Guid id, [FromBody] DisableRequest req, CancellationToken ct)
    {
        await _mediator.Send(new DisableSubscriptionCommand(id, CurrentAdminId, req.Reason), ct);
        return Ok(ApiResponse.Ok("Đã vô hiệu hóa dịch vụ Subscribe."));
    }

    // ──────── UC21: Lock user ─────────────────────────────────────
    [HttpPost("users/{id:guid}/lock")]
    public async Task<IActionResult> LockUser(
        Guid id, [FromBody] LockRequest req, CancellationToken ct)
    {
        await _mediator.Send(new LockUserCommand(id, CurrentAdminId, req.Reason), ct);
        return Ok(ApiResponse.Ok("Đã khóa tài khoản."));
    }

    // ──────── UC21: Get users ─────────────────────────────────────
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search, [FromQuery] string? subscriptionStatus,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetAdminUsersQuery(search, subscriptionStatus, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<AdminUserDto>>.OkPaged(result, result.Meta));
    }

    // ──────── UC21: Unlock user ───────────────────────────────────
    [HttpPost("users/{id:guid}/unlock")]
    public async Task<IActionResult> UnlockUser(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new UnlockUserCommand(id), ct);
        return Ok(ApiResponse.Ok("Đã mở khóa tài khoản."));
    }

    // ──────── UC11: Get feedbacks ─────────────────────────────────
    [HttpGet("feedbacks")]
    public async Task<IActionResult> GetFeedbacks(
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetFeedbacksQuery(status, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<FeedbackDto>>.OkPaged(result, result.Meta));
    }

    [HttpPatch("feedbacks/{id:guid}/read")]
    public async Task<IActionResult> MarkFeedbackAsRead(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new EGreetings.Application.Commands.Admin.Feedbacks.MarkFeedbackAsReadCommand(id), ct);
        return Ok(ApiResponse.Ok("Đã đánh dấu phản hồi là đã xử lý."));
    }

    // ──────── UC12: Transaction report ────────────────────────────
    [HttpGet("reports/transactions")]
    public async Task<IActionResult> GetTransactionReport(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetTransactionReportQuery(from, to, search, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<TransactionDto>>.OkPaged(result, result.Meta));
    }

    // ──────── UC30: System logs ───────────────────────────────────
    [HttpGet("logs")]
    public async Task<IActionResult> GetSystemLogs(
        [FromQuery] string? eventType, [FromQuery] string? status,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetSystemLogsQuery(eventType, status, from, to, page, pageSize), ct);
        return Ok(ApiResponse<PagedResult<SystemLogDto>>.OkPaged(result, result.Meta));
    }
}

public record DisableRequest(string Reason);
public record LockRequest(string Reason);
